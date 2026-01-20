using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Modelos;
using Identidade.Dominio.Servicos;
using Identidade.Infraestrutura.Helpers;
using Identidade.Publico.Dtos;
using Identidade.Publico.Events;
using MassTransit;
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
        }

        public async Task<OutputUserGroupDto> AddPermissions(string userGroupName, IReadOnlyCollection<InputPermissionDto> permissions, string requestUserId)
        {
            var dicPermissions = permissions.ToDictionary(p => p.Id, p => _permissaoOperacaoHelper.GetSomaOperacoes(p.Operations));
            var userGroup = await _authorizationService.AddPermissionsIntoUserGroup(userGroupName, dicPermissions);
            var userGroupDto = _fabricaGrupoUsuario.MapearParaDtoSaidaGrupoUsuario(userGroup);

            await _bus.Publish(
                new UserGroupCreatedOrUpdatedEvent
                {
                    UserGroup = userGroupDto,
                    RequestUserId = requestUserId
                });

            return userGroupDto;
        }

        public async Task<OutputUserGroupDto> Create(InputUserGroupDto userGroupDto, string requestUserId, string suggestedId = null)
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
            await _bus.Publish(
                new UserGroupCreatedOrUpdatedEvent
                {
                    UserGroup = createdUserGroupDto,
                    RequestUserId = requestUserId
                });

            return createdUserGroupDto;
        }
        private bool CreateUpdateGroupUserARC(UserGroup createdUserGroup, InputUserGroupDto userGroupDto)
        {
            _userGroupRepository.ConfigureParametersToCreateUpdate(out var parameters, createdUserGroup.Name, Constants.DATA_MAXIMA(), createdUserGroup.Id, userGroupDto.ArcXml, Constants.cst_GrupoUsuario, Constants.cst_Ugr);
            var context = _userGroupRepository.CreateUpdateItemDirectory(Constants.cst_SpAtualizaItemDiretorio, parameters);

            return context;
        }

        public async Task Delete(string userGroupName, string requestUserId)
        {
            _databaseConnectionModifier.ModifyConnection(_userGroupRepository.GetContext(), requestUserId);
            _userGroupRepository.BeginTransaction();

            var deletedId = await _userGroupRepository.RemoveByName(userGroupName);

            bool successfulResult = false;
            _userGroupRepository.ConfigureParametersToRemove(out var parameters, deletedId);
            successfulResult = _userGroupRepository.RemoveItemDirectory(Constants.cst_SpRemoveItemDiretorio, parameters);

            _userGroupRepository.DefineTransaction(successfulResult);
            if(successfulResult)
                await _bus.Publish(
                    new UserGroupDeletedEvent
                    {
                        UserGroupId = deletedId,
                        RequestUserId = requestUserId
                    });
        }

        public async Task<OutputUserGroupDto> DeletePermissions(string userGroupName, IReadOnlyCollection<string> permissionsIds, string requestUserId)
        {
            var userGroup = await _authorizationService.DeletePermissionsFromUserGroup(userGroupName, permissionsIds);
            var userGroupDto = _fabricaGrupoUsuario.MapearParaDtoSaidaGrupoUsuario(userGroup);

            await _bus.Publish(
                new UserGroupCreatedOrUpdatedEvent
                {
                    UserGroup = userGroupDto,
                    RequestUserId = requestUserId
                });

            return userGroupDto;
        }

        public async Task<IReadOnlyCollection<OutputUserGroupDto>> Get(string userGroupName)
        {
            if (userGroupName == null)
                return await GetAll();

            return await GetByName(userGroupName);
        }

        public async Task<OutputUserGroupDto> GetById(string userGroupId)
        {
            var userGroup = await _userGroupRepository.GetById(userGroupId);
            return _fabricaGrupoUsuario.MapearParaDtoSaidaGrupoUsuario(userGroup);
        }

        public async Task<IReadOnlyCollection<OutputPermissionDto>> GetPermissions(string userGroupName)
        {
            var userGroup = await _userGroupRepository.GetByName(userGroupName);
            var permissions = userGroup.UserGroupPermissions.Select(ugp => ugp.Permission).ToArray();
            var permissionsDto = _fabricaPermissao.MapearParaDtoSaidaPermissao(permissions);

            return permissionsDto;
        }

        public async Task<OutputUserGroupDto> Update(string userGroupId, InputUserGroupDto userGroupDto, string requestUserId)
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

            await _bus.Publish(
                new UserGroupCreatedOrUpdatedEvent
                {
                    UserGroup = createdUserGroupDto,
                    RequestUserId = requestUserId
                });

            return createdUserGroupDto;
        }

        public async Task<OutputUserGroupDto> UpdateByName(string userGroupName, InputUserGroupDto userGroupDto, string requestUserId)
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

        public async Task<OutputUserGroupDto> CreateApi(InputUserGroupDto userGroupDto, string requestUserId, string suggestedId = null)
        {
            _databaseConnectionModifier.ModifyConnection(_userGroupRepository.GetContext(), requestUserId);

            var userGroup = await _fabricaGrupoUsuario.MapearParaGrupoUsuarioAsync(userGroupDto);
            userGroup.Id = suggestedId;

            var createdUserGroup = await _userGroupRepository.Create(userGroup);
            var createdUserGroupDto = _fabricaGrupoUsuario.MapearParaDtoSaidaGrupoUsuario(createdUserGroup);

            await _bus.Publish(
                new UserGroupCreatedOrUpdatedEvent
                {
                    UserGroup = createdUserGroupDto,
                    RequestUserId = requestUserId
                });

            return createdUserGroupDto;
        }
        public async Task DeleteApi(string userGroupId, string requestUserId)
        {
            _databaseConnectionModifier.ModifyConnection(_userGroupRepository.GetContext(), requestUserId);
            await _userGroupRepository.Remove(userGroupId);

            await _bus.Publish(
                new UserGroupDeletedEvent
                {
                    UserGroupId = userGroupId,
                    RequestUserId = requestUserId
                });
        }
        public async Task<OutputUserGroupDto> UpdateApi(string userGroupId, InputUserGroupDto userGroupDto, string requestUserId)
        {
            _databaseConnectionModifier.ModifyConnection(_userGroupRepository.GetContext(), requestUserId);
            var userGroup = await _userGroupRepository.GetById(userGroupId);

            if (userGroupDto?.Name != null)
                userGroup.Name = userGroupDto.Name;

            if (userGroupDto?.Permissions != null)
                userGroup.UserGroupPermissions = await _fabricaGrupoUsuario.ConstruirPermissoesGrupoUsuarioAsync(userGroup, userGroupDto.Permissions);

            var savedUserGroup = await _userGroupRepository.Update(userGroup);
            var savedUserGroupDto = _fabricaGrupoUsuario.MapearParaDtoSaidaGrupoUsuario(savedUserGroup);

            await _bus.Publish(
                new UserGroupCreatedOrUpdatedEvent
                {
                    UserGroup = savedUserGroupDto,
                    RequestUserId = requestUserId
                });

            return savedUserGroupDto;
        }
    }
}
