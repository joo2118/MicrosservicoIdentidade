using AutoMapper;
using MassTransit;
using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Modelos;
using Identidade.Dominio.Servicos;
using Identidade.Infraestrutura.Helpers;
using Identidade.Publico.Dtos;
using Identidade.Publico.Events;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Identidade.Dominio.Modelos;

namespace Identidade.Infraestrutura.ClientServices
{
    public interface IUserGroupClientService
    {
        Task<OutputUserGroupDto> AddPermissions(string userGroupId, IReadOnlyCollection<InputPermissionDto> permissions, string requestUserId);
        Task<OutputUserGroupDto> Create(InputUserGroupDto userGroupDto, string requestUserId, string suggestedId = null);
        Task<OutputUserGroupDto> CreateApi(InputUserGroupDto userGroupDto, string requestUserId, string suggestedId = null);
        Task Delete(string userGroupId, string requestUserId);
        Task DeleteApi(string userGroupId, string requestUserId);
        Task<OutputUserGroupDto> DeletePermissions(string userGroupId, IReadOnlyCollection<string> permissionsIds, string requestUserId);
        Task<IReadOnlyCollection<OutputUserGroupDto>> Get(string userGroupName);
        Task<OutputUserGroupDto> GetById(string userGroupId);
        Task<IReadOnlyCollection<OutputPermissionDto>> GetPermissions(string userGroupId);
        Task<OutputUserGroupDto> Update(string userGroupId, InputUserGroupDto userGroupDto, string requestUserId);
        Task<OutputUserGroupDto> UpdateApi(string userGroupId, InputUserGroupDto userGroupDto, string requestUserId);
    }

    public class UserGroupClientService : IUserGroupClientService
    {
        private readonly IRepository<UserGroup> _userGroupRepository;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMapper _mapper;
        private readonly IBus _bus;
        private readonly IPermissionOperationManager _permissionOperationManager;
        private readonly IDatabaseConnectionUserModifier _databaseConnectionModifier;

        public UserGroupClientService(IRepository<UserGroup> userGroupRepository, IAuthorizationService authorizationService,
            IMapper mapper, IBus bus, IPermissionOperationManager permissionOperationManager, IDatabaseConnectionUserModifier databaseConnectionModifier)
        {
            _userGroupRepository = userGroupRepository;
            _authorizationService = authorizationService;
            _mapper = mapper;
            _bus = bus;
            _permissionOperationManager = permissionOperationManager;
            _databaseConnectionModifier = databaseConnectionModifier;
        }

        public async Task<OutputUserGroupDto> AddPermissions(string userGroupId, IReadOnlyCollection<InputPermissionDto> permissions, string requestUserId)
        {
            var dicPermissions = permissions.ToDictionary(p => p.Id, p => _permissionOperationManager.GetOperationSum(p.Operations));
            var userGroup = await _authorizationService.AddPermissionsIntoUserGroup(userGroupId, dicPermissions);
            var userGroupDto = _mapper.Map<UserGroup, OutputUserGroupDto>(userGroup);

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

            var userGroup = _mapper.Map<InputUserGroupDto, UserGroup>(userGroupDto);

            userGroup.Id = suggestedId;
            var createdUserGroup = await _userGroupRepository.Create(userGroup);

            bool successfulResult = false;
            if (createdUserGroup != null)
            {
                successfulResult = CreateUpdateGroupUserARC(createdUserGroup, userGroupDto);
            }

            _userGroupRepository.DefineTransaction(successfulResult);

            var createdUserGroupDto = _mapper.Map<UserGroup, OutputUserGroupDto>(createdUserGroup);

            return createdUserGroupDto;
        }
        private bool CreateUpdateGroupUserARC(UserGroup createdUserGroup, InputUserGroupDto userGroupDto)
        {
            _userGroupRepository.ConfigureParametersToCreateUpdate(out var parameters, createdUserGroup.Name, Constants.DATA_MAXIMA(), createdUserGroup.Id, userGroupDto.ArcXml, Constants.cst_GrupoUsuario, Constants.cst_Ugr);
            var context = _userGroupRepository.CreateUpdateItemDirectory(Constants.cst_SpAtualizaItemDiretorio, parameters);

            return context;
        }

        public async Task Delete(string userGroupId, string requestUserId)
        {
            _databaseConnectionModifier.ModifyConnection(_userGroupRepository.GetContext(), requestUserId);
            _userGroupRepository.BeginTransaction();

            var groupDeleted = await _userGroupRepository.Remove(userGroupId);

            bool successfulResult = false;
            if (groupDeleted)
            {
                _userGroupRepository.ConfigureParametersToRemove(out var parameters, userGroupId);
                successfulResult = _userGroupRepository.RemoveItemDirectory(Constants.cst_SpRemoveItemDiretorio, parameters);
            }

            _userGroupRepository.DefineTransaction(successfulResult);
        }

        public async Task<OutputUserGroupDto> DeletePermissions(string userGroupId, IReadOnlyCollection<string> permissionsIds, string requestUserId)
        {
            var userGroup = await _authorizationService.DeletePermissionsFromUserGroup(userGroupId, permissionsIds);
            var userGroupDto = _mapper.Map<UserGroup, OutputUserGroupDto>(userGroup);

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
            var userGroupInfo = _mapper.Map<UserGroup, OutputUserGroupDto>(userGroup);

            return userGroupInfo;
        }

        public async Task<IReadOnlyCollection<OutputPermissionDto>> GetPermissions(string userGroupId)
        {
            var userGroup = await _userGroupRepository.GetById(userGroupId);
            var permissions = userGroup.UserGroupPermissions.Select(ugp => ugp.Permission).ToArray();
            var permissionsDto = _mapper.Map<Permission[], OutputPermissionDto[]>(permissions);

            return permissionsDto;
        }

        public async Task<OutputUserGroupDto> Update(string userGroupId, InputUserGroupDto userGroupDto, string requestUserId)
        {
            _databaseConnectionModifier.ModifyConnection(_userGroupRepository.GetContext(), requestUserId);
            _userGroupRepository.BeginTransaction();

            var userGroup = await _userGroupRepository.GetById(userGroupId);
            var updatedUserGroup = _mapper.Map(userGroupDto, userGroup);

            var savedUserGroup = await _userGroupRepository.Update(updatedUserGroup);
            var savedUserGroupDto = _mapper.Map<UserGroup, OutputUserGroupDto>(savedUserGroup);

            bool successfulResult = false;
            if (savedUserGroup != null)
            {
                successfulResult = CreateUpdateGroupUserARC(savedUserGroup, userGroupDto);
            }

            _userGroupRepository.DefineTransaction(successfulResult);

            return savedUserGroupDto;
        }

        private async Task<IReadOnlyCollection<OutputUserGroupDto>> GetAll()
        {
            var userGroups = await _userGroupRepository.GetAll();
            var userGroupDtos = _mapper.Map<IReadOnlyCollection<UserGroup>, IReadOnlyCollection<OutputUserGroupDto>>(userGroups);

            return userGroupDtos;
        }

        private async Task<IReadOnlyCollection<OutputUserGroupDto>> GetByName(string userGroupName)
        {
            var userGroup = await _userGroupRepository.GetByName(userGroupName);
            var userGroupDto = _mapper.Map<UserGroup, OutputUserGroupDto>(userGroup);

            return new[] { userGroupDto };
        }
        public async Task<OutputUserGroupDto> CreateApi(InputUserGroupDto userGroupDto, string requestUserId, string suggestedId = null)
        {
            _databaseConnectionModifier.ModifyConnection(_userGroupRepository.GetContext(), requestUserId);
            var userGroup = _mapper.Map<InputUserGroupDto, UserGroup>(userGroupDto);

            userGroup.Id = suggestedId;
            var createdUserGroup = await _userGroupRepository.Create(userGroup);
            var createdUserGroupDto = _mapper.Map<UserGroup, OutputUserGroupDto>(createdUserGroup);

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
            var updatedUserGroup = _mapper.Map(userGroupDto, userGroup);

            var savedUserGroup = await _userGroupRepository.Update(updatedUserGroup);
            var savedUserGroupDto = _mapper.Map<UserGroup, OutputUserGroupDto>(savedUserGroup);

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
