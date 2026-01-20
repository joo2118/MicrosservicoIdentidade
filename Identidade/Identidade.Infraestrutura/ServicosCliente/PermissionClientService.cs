using Identidade.Dominio.Helpers;
using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Modelos;
using Identidade.Dominio.Servicos;
using Identidade.Infraestrutura.Resilience;
using Identidade.Publico.Dtos;
using Microsoft.Data.SqlClient;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Identidade.Infraestrutura.ServicosCliente
{
    public interface IPermissionClientService
    {
        Task<OutputPermissionDto> GetById(string permissionId);
        Task<IReadOnlyCollection<OutputPermissionDto>> Get(string permissionName);
        Task<IReadOnlyCollection<OutputPermissionDto>> Get(string permissionName, int? page, int? pageSize);
        Task<IReadOnlyCollection<OutputUserGroupDto>> GetUserGroups(string permissionId);
        Task<IReadOnlyCollection<OutputUserGroupDto>> GetUserGroups(string permissionId, int? page, int? pageSize);
        Task<ResultadoPaginado<OutputPermissionDto>> GetPaginado(string permissionName, int? page, int? pageSize);
        Task<ResultadoPaginado<OutputUserGroupDto>> GetUserGroupsPaginado(string permissionId, int? page, int? pageSize);
    }

    public class PermissionClientService : IPermissionClientService
    {
        private readonly IReadOnlyRepository<Permission> _permissionRepository;
        private readonly IAuthorizationService _authorizationService;
        private readonly IFabricaPermissao _fabricaPermissao;
        private readonly ResiliencePipeline _pipeline;

        public PermissionClientService(IReadOnlyRepository<Permission> permissionRepository, IAuthorizationService authorizationService, IFabricaPermissao fabricaPermissao)
        {
            _permissionRepository = permissionRepository;
            _authorizationService = authorizationService;
            _fabricaPermissao = fabricaPermissao;

            _pipeline = FabricaPipelineResiliencia.Create();
        }

        public Task<OutputPermissionDto> GetById(string permissionId) =>
            ExecuteResilientAsync(() => GetByIdCore(permissionId));

        private async Task<OutputPermissionDto> GetByIdCore(string permissionId)
        {
            var permission = await _permissionRepository.GetById(permissionId);
            return _fabricaPermissao.MapearParaDtoSaidaPermissao(permission);
        }

        public Task<IReadOnlyCollection<OutputPermissionDto>> Get(string permissionName) =>
            Get(permissionName, page: null, pageSize: null);

        public Task<IReadOnlyCollection<OutputPermissionDto>> Get(string permissionName, int? page, int? pageSize) =>
            ExecuteResilientAsync(() => GetCore(permissionName, page, pageSize));

        private async Task<IReadOnlyCollection<OutputPermissionDto>> GetCore(string permissionName, int? page, int? pageSize)
        {
            if (permissionName == null)
                return await GetAll(page, pageSize);

            return await GetByName(permissionName);
        }

        public Task<IReadOnlyCollection<OutputUserGroupDto>> GetUserGroups(string permissionId) =>
            GetUserGroups(permissionId, page: null, pageSize: null);

        public Task<IReadOnlyCollection<OutputUserGroupDto>> GetUserGroups(string permissionId, int? page, int? pageSize) =>
            ExecuteResilientAsync(() => GetUserGroupsCore(permissionId, page, pageSize));

        private async Task<IReadOnlyCollection<OutputUserGroupDto>> GetUserGroupsCore(string permissionId, int? page, int? pageSize)
        {
            var userGroups = (await _authorizationService.GetUserGroupsContainigPermission(permissionId)).AsQueryable();

            if (page.HasValue || pageSize.HasValue)
            {
                var pagination = new OpcoesPaginacao(page, pageSize);
                userGroups = userGroups.Skip(pagination.Skip).Take(pagination.TamanhoPagina);
            }

            return userGroups
                .Select(ug => new OutputUserGroupDto { Id = ug.Id, Name = ug.Name, CreatedAt = ug.CreatedAt, LastUpdatedAt = ug.LastUpdatedAt })
                .ToArray();
        }

        public Task<ResultadoPaginado<OutputPermissionDto>> GetPaginado(string permissionName, int? page, int? pageSize) =>
            ExecuteResilientAsync(() => GetPaginadoCore(permissionName, page, pageSize));

        private async Task<ResultadoPaginado<OutputPermissionDto>> GetPaginadoCore(string permissionName, int? page, int? pageSize)
        {
            if (permissionName == null)
                return await GetAllPaginado(page, pageSize);

            var items = await GetByName(permissionName);
            return new ResultadoPaginado<OutputPermissionDto>
            {
                Items = items,
                Pagina = 1,
                TamanhoPagina = items.Count,
                Total = items.Count
            };
        }

        public Task<ResultadoPaginado<OutputUserGroupDto>> GetUserGroupsPaginado(string permissionId, int? page, int? pageSize) =>
            ExecuteResilientAsync(() => GetUserGroupsPaginadoCore(permissionId, page, pageSize));

        private async Task<ResultadoPaginado<OutputUserGroupDto>> GetUserGroupsPaginadoCore(string permissionId, int? page, int? pageSize)
        {
            var pagination = new OpcoesPaginacao(page, pageSize);

            var userGroups = (await _authorizationService.GetUserGroupsContainigPermission(permissionId)).AsQueryable();
            return await userGroups
                .Select(ug => new OutputUserGroupDto { Id = ug.Id, Name = ug.Name, CreatedAt = ug.CreatedAt, LastUpdatedAt = ug.LastUpdatedAt })
                .ParaResultadoPaginado(pagination);
        }

        private async Task<ResultadoPaginado<OutputPermissionDto>> GetAllPaginado(int? page, int? pageSize)
        {
            var pagination = new OpcoesPaginacao(page, pageSize);
            var permissionsQuery = (await _permissionRepository.GetAll()).AsQueryable();

            return await permissionsQuery
                .Select(p => _fabricaPermissao.MapearParaDtoSaidaPermissao(p))
                .ParaResultadoPaginado(pagination);
        }

        private async Task<IReadOnlyCollection<OutputPermissionDto>> GetAll(int? page, int? pageSize)
        {
            var permissions = await _permissionRepository.GetAll(page, pageSize);
            return permissions.Select(_fabricaPermissao.MapearParaDtoSaidaPermissao).ToArray();
        }

        private async Task<IReadOnlyCollection<OutputPermissionDto>> GetByName(string permissionName)
        {
            var permission = await _permissionRepository.GetByName(permissionName);
            return new[] { _fabricaPermissao.MapearParaDtoSaidaPermissao(permission) };
        }

        private Task<T> ExecuteResilientAsync<T>(Func<Task<T>> action) =>
            _pipeline.ExecuteAsync(async _ =>
            {
                try
                {
                    return await action().ConfigureAwait(false);
                }
                catch (SqlException ex) when (DetectorErroSQLTransitorio.ErroTransient(ex))
                {
                    throw new ExceptionTransitoriaSQL("Transient SQL error.", ex);
                }
            }).AsTask();
    }
}
