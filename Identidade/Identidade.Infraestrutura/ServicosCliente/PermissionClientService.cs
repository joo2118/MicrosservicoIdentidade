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
        Task<IReadOnlyCollection<OutputUserGroupDto>> GetUserGroups(string permissionId);
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
            ExecuteResilientAsync(() => GetCore(permissionName));

        private async Task<IReadOnlyCollection<OutputPermissionDto>> GetCore(string permissionName)
        {
            if (permissionName == null)
                return await GetAll();

            return await GetByName(permissionName);
        }

        public Task<IReadOnlyCollection<OutputUserGroupDto>> GetUserGroups(string permissionId) =>
            ExecuteResilientAsync(() => GetUserGroupsCore(permissionId));

        private async Task<IReadOnlyCollection<OutputUserGroupDto>> GetUserGroupsCore(string permissionId)
        {
            var userGroups = await _authorizationService.GetUserGroupsContainigPermission(permissionId);
            return userGroups.Select(ug => new OutputUserGroupDto { Id = ug.Id, Name = ug.Name, CreatedAt = ug.CreatedAt, LastUpdatedAt = ug.LastUpdatedAt }).ToArray();
        }

        private async Task<IReadOnlyCollection<OutputPermissionDto>> GetAll()
        {
            var permissions = await _permissionRepository.GetAll();
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
