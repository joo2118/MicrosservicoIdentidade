using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Identidade.Dominio.Helpers;
using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Modelos;
using Identidade.Infraestrutura.Helpers;
using Identidade.Publico.Dtos;

namespace Identidade.Infraestrutura.Data
{
    public class UserGroupProfile : Profile
    {
        private readonly IReadOnlyRepository<UserGroup> _userGroupRepository;
        private readonly IReadOnlyRepository<Permission> _permissionRepository;
        private readonly IPermissionOperationManager _permissionOperationManager;

        public UserGroupProfile(IReadOnlyRepository<UserGroup> userGroupRepository, IReadOnlyRepository<Permission> permissionRepository, IPermissionOperationManager permissionOperationManager)
        {
            _userGroupRepository = userGroupRepository;
            _permissionRepository = permissionRepository;
            _permissionOperationManager = permissionOperationManager;

            CreateMap<UserGroup, OutputUserGroupDto>()
                .ForMember(userGroupDto => userGroupDto.Permissions,
                    ex => ex.MapFrom(userGroup => GetInputPermissionDtos(userGroup)));

            CreateMap<InputUserGroupDto, UserGroup>()
                .ForMember(userGroup => userGroup.UserGroupPermissions,
                    ex => ex.MapFrom(userGroupDto => GetUserGroupPermissions(userGroupDto)))
                .ForAllMembers(option => option.Condition((source, destination, sourceMember) => sourceMember != null));
        }

        private ICollection<UserGroupPermission> GetUserGroupPermissions(InputUserGroupDto userGroupDto) =>
            userGroupDto.Permissions?.Select(permissionDto => GetUserGroupPermission(userGroupDto, permissionDto)).ToList();

        private UserGroupPermission GetUserGroupPermission(InputUserGroupDto userGroupDto, InputPermissionDto permissionDto)
        {
            var userGroup = ExceptionCatcher.ExecuteSafe<NotFoundAppException, UserGroup>(
                () => _userGroupRepository.GetByName(userGroupDto.Name).GetAwaiter().GetResult(),
                e => { });

            var permission = _permissionRepository.GetById(permissionDto.Id).GetAwaiter().GetResult();
            int permissionOperations = _permissionOperationManager.GetOperationSum(permissionDto.Operations);

            return new UserGroupPermission(userGroup, permission, permissionOperations);
        }

        private List<InputPermissionDto> GetInputPermissionDtos(UserGroup userGroup) =>
            userGroup.UserGroupPermissions?.Select(GetInputPermissionDto).ToList() ?? new List<InputPermissionDto>();

        private InputPermissionDto GetInputPermissionDto(UserGroupPermission ugp) =>
            new InputPermissionDto
            {
                Id = ugp.PermissionId,
                Operations = _permissionOperationManager.GetOperations(ugp.PermissionOperations)
            };
    }
}
