using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Modelos;
using Identidade.Dominio.Servicos;
using Identidade.Infraestrutura.Helpers;
using Identidade.Infraestrutura.Resilience;
using Identidade.Publico.Dtos;
using Identidade.Publico.Events;
using MassTransit;
using Microsoft.Data.SqlClient;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Identidade.Infraestrutura.ServicosCliente
{
    public interface IUserGroupClientService
    {
        Task<OutputUserGroupDto> AddPermissions(string userGroupName, IReadOnlyCollection<InputPermissionDto> permissions, string requestUserId);
        Task<OutputUserGroupDto> Create(InputUserGroupDto userGroupDto, string requestUserId, string suggestedId = null);
        Task<OutputUserGroupDto> CreateApi(InputUserGroupDto userGroupDto, string requestUserId, string suggestedId = null);
        Task Delete(string userGroupName, string requestUserId);
        Task DeleteApi(string userGroupId, string requestUserId);
        Task<OutputUserGroupDto> DeletePermissions(string userGroupName, IReadOnlyCollection<string> permissionsIds, string requestUserId);
        Task<IReadOnlyCollection<OutputUserGroupDto>> Get(string userGroupName);
        Task<OutputUserGroupDto> GetById(string userGroupId);
        Task<IReadOnlyCollection<OutputPermissionDto>> GetPermissions(string userGroupName);
        Task<OutputUserGroupDto> Update(string userGroupId, InputUserGroupDto userGroupDto, string requestUserId);
        Task<OutputUserGroupDto> UpdateByName(string userGroupName, InputUserGroupDto userGroupDto, string requestUserId);
        Task<OutputUserGroupDto> UpdateApi(string userGroupId, InputUserGroupDto userGroupDto, string requestUserId);
    }

    public class UserGroupClientService : IUserGroupClientService
    {
        private readonly IRepository<UserGroup> _userGroupRepository;
        private readonly IAuthorizationService _authorizationService;
        private readonly IBus _bus;
        private readonly IPermissaoOperacaoHelper _permissaoOperacaoHelper;
        private readonly IDatabaseConnectionUserModifier _databaseConnectionModifier;
        private readonly IFabricaGrupoUsuario _fabricaGrupoUsuario;
        private readonly IFabricaPermissao _fabricaPermissao;
        private readonly ResiliencePipeline _pipeline;
        private readonly ResiliencePipeline _busPublishPipeline;

        public UserGroupClientService(IRepository<UserGroup> userGroupRepository, IAuthorizationService authorizationService,
            IBus bus, IPermissaoOperacaoHelper permissaoOperacaoHelper, IDatabaseConnectionUserModifier databaseConnectionModifier,
            IFabricaGrupoUsuario fabricaGrupoUsuario, IFabricaPermissao fabricaPermissao)
        {
            _userGroupRepository = userGroupRepository;
            _authorizationService = authorizationService;
            _bus = bus;
            _permissaoOperacaoHelper = permissaoOperacaoHelper;
            _databaseConnectionModifier = databaseConnectionModifier;
            _fabricaGrupoUsuario = fabricaGrupoUsuario;
            _fabricaPermissao = fabricaPermissao;

            _pipeline = FabricaPipelineResiliencia.Create();
            _busPublishPipeline = FabricaPipelineResiliencia.CreateBusPublish();
        }

        public Task<OutputUserGroupDto> AddPermissions(string userGroupName, IReadOnlyCollection<InputPermissionDto> permissions, string requestUserId) =>
            ExecuteResilientAsync(() => AddPermissionsCore(userGroupName, permissions, requestUserId));

        private async Task<OutputUserGroupDto> AddPermissionsCore(string userGroupName, IReadOnlyCollection<InputPermissionDto> permissions, string requestUserId)
        {
            var dicPermissions = permissions.ToDictionary(p => p.Id, p => _permissaoOperacaoHelper.GetSomaOperacoes(p.Operations));
            var userGroup = await _authorizationService.AddPermissionsIntoUserGroup(userGroupName, dicPermissions);
            var userGroupDto = _fabricaGrupoUsuario.MapearParaDtoSaidaGrupoUsuario(userGroup);

            await ExecuteResilientBusPublishAsync(() => _bus.Publish(
                new UserGroupCreatedOrUpdatedEvent
                {
                    UserGroup = userGroupDto,
                    RequestUserId = requestUserId
                }));

            return userGroupDto;
        }

        public Task<OutputUserGroupDto> Create(InputUserGroupDto userGroupDto, string requestUserId, string suggestedId = null) =>
            ExecuteResilientAsync(() => CreateCore(userGroupDto, requestUserId, suggestedId));

        private async Task<OutputUserGroupDto> CreateCore(InputUserGroupDto userGroupDto, string requestUserId, string suggestedId = null)
        {
            _databaseConnectionModifier.ModifyConnection(_userGroupRepository.GetContext(), requestUserId);
            _userGroupRepository.BeginTransaction();

            var userGroup = await _fabricaGrupoUsuario.MapearParaGrupoUsuarioAsync(userGroupDto);
            userGroup.Id = suggestedId;

            var createdUserGroup = await _userGroupRepository.Create(userGroup);

            bool successfulResult = false;
            if (createdUserGroup != null)
            {
                successfulResult = CreateUpdateGroupUserARC(createdUserGroup, userGroupDto);
            }

            _userGroupRepository.DefineTransaction(successfulResult);

            var createdUserGroupDto = _fabricaGrupoUsuario.MapearParaDtoSaidaGrupoUsuario(createdUserGroup);
            await ExecuteResilientBusPublishAsync(() => _bus.Publish(
                new UserGroupCreatedOrUpdatedEvent
                {
                    UserGroup = createdUserGroupDto,
                    RequestUserId = requestUserId
                }));

            return createdUserGroupDto;
        }

        private bool CreateUpdateGroupUserARC(UserGroup createdUserGroup, InputUserGroupDto userGroupDto)
        {
            _userGroupRepository.ConfigureParametersToCreateUpdate(out var parameters, createdUserGroup.Name, Constants.DATA_MAXIMA(), createdUserGroup.Id, userGroupDto.ArcXml, Constants.cst_GrupoUsuario, Constants.cst_Ugr);
            var context = _userGroupRepository.CreateUpdateItemDirectory(Constants.cst_SpAtualizaItemDiretorio, parameters);

            return context;
        }

        public Task Delete(string userGroupName, string requestUserId) =>
            ExecuteResilientAsync(() => DeleteCore(userGroupName, requestUserId));

        private async Task DeleteCore(string userGroupName, string requestUserId)
        {
            _databaseConnectionModifier.ModifyConnection(_userGroupRepository.GetContext(), requestUserId);
            _userGroupRepository.BeginTransaction();

            var deletedId = await _userGroupRepository.RemoveByName(userGroupName);

            _userGroupRepository.ConfigureParametersToRemove(out var parameters, deletedId);
            var successfulResult = _userGroupRepository.RemoveItemDirectory(Constants.cst_SpRemoveItemDiretorio, parameters);

            _userGroupRepository.DefineTransaction(successfulResult);
            if (successfulResult)
                await ExecuteResilientBusPublishAsync(() => _bus.Publish(
                    new UserGroupDeletedEvent
                    {
                        UserGroupId = deletedId,
                        RequestUserId = requestUserId
                    }));
        }

        public Task<OutputUserGroupDto> DeletePermissions(string userGroupName, IReadOnlyCollection<string> permissionsIds, string requestUserId) =>
            ExecuteResilientAsync(() => DeletePermissionsCore(userGroupName, permissionsIds, requestUserId));

        private async Task<OutputUserGroupDto> DeletePermissionsCore(string userGroupName, IReadOnlyCollection<string> permissionsIds, string requestUserId)
        {
            var userGroup = await _authorizationService.DeletePermissionsFromUserGroup(userGroupName, permissionsIds);
            var userGroupDto = _fabricaGrupoUsuario.MapearParaDtoSaidaGrupoUsuario(userGroup);

            await ExecuteResilientBusPublishAsync(() => _bus.Publish(
                new UserGroupCreatedOrUpdatedEvent
                {
                    UserGroup = userGroupDto,
                    RequestUserId = requestUserId
                }));

            return userGroupDto;
        }

        public Task<IReadOnlyCollection<OutputUserGroupDto>> Get(string userGroupName) =>
            ExecuteResilientAsync(() => GetCore(userGroupName));

        private async Task<IReadOnlyCollection<OutputUserGroupDto>> GetCore(string userGroupName)
        {
            if (userGroupName == null)
                return await GetAll();

            return await GetByName(userGroupName);
        }

        public Task<OutputUserGroupDto> GetById(string userGroupId) =>
            ExecuteResilientAsync(() => GetByIdCore(userGroupId));

        private async Task<OutputUserGroupDto> GetByIdCore(string userGroupId)
        {
            var userGroup = await _userGroupRepository.GetById(userGroupId);
            return _fabricaGrupoUsuario.MapearParaDtoSaidaGrupoUsuario(userGroup);
        }

        public Task<IReadOnlyCollection<OutputPermissionDto>> GetPermissions(string userGroupName) =>
            ExecuteResilientAsync(() => GetPermissionsCore(userGroupName));

        private async Task<IReadOnlyCollection<OutputPermissionDto>> GetPermissionsCore(string userGroupName)
        {
            var userGroup = await _userGroupRepository.GetByName(userGroupName);
            var permissions = userGroup.UserGroupPermissions.Select(ugp => ugp.Permission).ToArray();
            var permissionsDto = _fabricaPermissao.MapearParaDtoSaidaPermissao(permissions);

            return permissionsDto;
        }

        public Task<OutputUserGroupDto> Update(string userGroupId, InputUserGroupDto userGroupDto, string requestUserId) =>
            ExecuteResilientAsync(() => UpdateCore(userGroupId, userGroupDto, requestUserId));

        private async Task<OutputUserGroupDto> UpdateCore(string userGroupId, InputUserGroupDto userGroupDto, string requestUserId)
        {
            _databaseConnectionModifier.ModifyConnection(_userGroupRepository.GetContext(), requestUserId);
            _userGroupRepository.BeginTransaction();

            var userGroup = await _userGroupRepository.GetById(userGroupId);

            if (userGroupDto?.Name != null)
                userGroup.Name = userGroupDto.Name;

            if (userGroupDto?.Permissions != null)
                userGroup.UserGroupPermissions = await _fabricaGrupoUsuario.ConstruirPermissoesGrupoUsuarioAsync(userGroup, userGroupDto.Permissions);

            var savedUserGroup = await _userGroupRepository.Update(userGroup);

            bool successfulResult = false;
            if (savedUserGroup != null)
            {
                successfulResult = CreateUpdateGroupUserARC(savedUserGroup, userGroupDto);
            }

            _userGroupRepository.DefineTransaction(successfulResult);

            var createdUserGroupDto = _fabricaGrupoUsuario.MapearParaDtoSaidaGrupoUsuario(savedUserGroup);

            await ExecuteResilientBusPublishAsync(() => _bus.Publish(
                new UserGroupCreatedOrUpdatedEvent
                {
                    UserGroup = createdUserGroupDto,
                    RequestUserId = requestUserId
                }));

            return createdUserGroupDto;
        }

        public Task<OutputUserGroupDto> UpdateByName(string userGroupName, InputUserGroupDto userGroupDto, string requestUserId) =>
            ExecuteResilientAsync(() => UpdateByNameCore(userGroupName, userGroupDto, requestUserId));

        private async Task<OutputUserGroupDto> UpdateByNameCore(string userGroupName, InputUserGroupDto userGroupDto, string requestUserId)
        {
            _databaseConnectionModifier.ModifyConnection(_userGroupRepository.GetContext(), requestUserId);
            _userGroupRepository.BeginTransaction();

            var userGroup = await _userGroupRepository.GetByName(userGroupName);

            if (userGroupDto?.Name != null)
                userGroup.Name = userGroupDto.Name;

            if (userGroupDto?.Permissions != null)
                userGroup.UserGroupPermissions = await _fabricaGrupoUsuario.ConstruirPermissoesGrupoUsuarioAsync(userGroup, userGroupDto.Permissions);

            var savedUserGroup = await _userGroupRepository.Update(userGroup);

            bool successfulResult = false;
            if (savedUserGroup != null)
            {
                successfulResult = CreateUpdateGroupUserARC(savedUserGroup, userGroupDto);
            }

            _userGroupRepository.DefineTransaction(successfulResult);

            return _fabricaGrupoUsuario.MapearParaDtoSaidaGrupoUsuario(savedUserGroup);
        }

        private async Task<IReadOnlyCollection<OutputUserGroupDto>> GetAll()
        {
            var userGroups = await _userGroupRepository.GetAll();
            return userGroups.Select(_fabricaGrupoUsuario.MapearParaDtoSaidaGrupoUsuario).ToArray();
        }

        private async Task<IReadOnlyCollection<OutputUserGroupDto>> GetByName(string userGroupName)
        {
            var userGroup = await _userGroupRepository.GetByName(userGroupName);
            return new[] { _fabricaGrupoUsuario.MapearParaDtoSaidaGrupoUsuario(userGroup) };
        }

        public Task<OutputUserGroupDto> CreateApi(InputUserGroupDto userGroupDto, string requestUserId, string suggestedId = null) =>
            ExecuteResilientAsync(() => CreateApiCore(userGroupDto, requestUserId, suggestedId));

        private async Task<OutputUserGroupDto> CreateApiCore(InputUserGroupDto userGroupDto, string requestUserId, string suggestedId = null)
        {
            _databaseConnectionModifier.ModifyConnection(_userGroupRepository.GetContext(), requestUserId);

            var userGroup = await _fabricaGrupoUsuario.MapearParaGrupoUsuarioAsync(userGroupDto);
            userGroup.Id = suggestedId;

            var createdUserGroup = await _userGroupRepository.Create(userGroup);
            var createdUserGroupDto = _fabricaGrupoUsuario.MapearParaDtoSaidaGrupoUsuario(createdUserGroup);

            await ExecuteResilientBusPublishAsync(() => _bus.Publish(
                new UserGroupCreatedOrUpdatedEvent
                {
                    UserGroup = createdUserGroupDto,
                    RequestUserId = requestUserId
                }));

            return createdUserGroupDto;
        }

        public Task DeleteApi(string userGroupId, string requestUserId) =>
            ExecuteResilientAsync(() => DeleteApiCore(userGroupId, requestUserId));

        private async Task DeleteApiCore(string userGroupId, string requestUserId)
        {
            _databaseConnectionModifier.ModifyConnection(_userGroupRepository.GetContext(), requestUserId);
            await _userGroupRepository.Remove(userGroupId);

            await ExecuteResilientBusPublishAsync(() => _bus.Publish(
                new UserGroupDeletedEvent
                {
                    UserGroupId = userGroupId,
                    RequestUserId = requestUserId
                }));
        }

        public Task<OutputUserGroupDto> UpdateApi(string userGroupId, InputUserGroupDto userGroupDto, string requestUserId) =>
            ExecuteResilientAsync(() => UpdateApiCore(userGroupId, userGroupDto, requestUserId));

        private async Task<OutputUserGroupDto> UpdateApiCore(string userGroupId, InputUserGroupDto userGroupDto, string requestUserId)
        {
            _databaseConnectionModifier.ModifyConnection(_userGroupRepository.GetContext(), requestUserId);
            var userGroup = await _userGroupRepository.GetById(userGroupId);

            if (userGroupDto?.Name != null)
                userGroup.Name = userGroupDto.Name;

            if (userGroupDto?.Permissions != null)
                userGroup.UserGroupPermissions = await _fabricaGrupoUsuario.ConstruirPermissoesGrupoUsuarioAsync(userGroup, userGroupDto.Permissions);

            var savedUserGroup = await _userGroupRepository.Update(userGroup);
            var savedUserGroupDto = _fabricaGrupoUsuario.MapearParaDtoSaidaGrupoUsuario(savedUserGroup);

            await ExecuteResilientBusPublishAsync(() => _bus.Publish(
                new UserGroupCreatedOrUpdatedEvent
                {
                    UserGroup = savedUserGroupDto,
                    RequestUserId = requestUserId
                }));

            return savedUserGroupDto;
        }

        private Task ExecuteResilientBusPublishAsync(Func<Task> action) =>
            _busPublishPipeline.ExecuteAsync(async _ =>
            {
                try
                {
                    await action().ConfigureAwait(false);
                }
                catch (SqlException ex) when (DetectorErroSQLTransitorio.ErroTransient(ex))
                {
                    throw new ExceptionTransitoriaSQL("Transient SQL error.", ex);
                }
            }).AsTask();

        private Task ExecuteResilientAsync(Func<Task> action) =>
            _pipeline.ExecuteAsync(async _ =>
            {
                try
                {
                    await action().ConfigureAwait(false);
                }
                catch (SqlException ex) when (DetectorErroSQLTransitorio.ErroTransient(ex))
                {
                    throw new ExceptionTransitoriaSQL("Transient SQL error.", ex);
                }
            }).AsTask();

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
