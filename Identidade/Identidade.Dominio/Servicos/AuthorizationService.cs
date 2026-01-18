using Identidade.Dominio.Helpers;
using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Modelos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserGroup = Identidade.Dominio.Modelos.UserGroup;

namespace Identidade.Dominio.Servicos
{
    public interface IAuthorizationService
    {
        Task<UserGroup> AddPermissionsIntoUserGroup(string userGroupName, IReadOnlyDictionary<string, int> permissions);
        Task<UserGroup> DeletePermissionsFromUserGroup(string userGroupName, IReadOnlyCollection<string> permissionsIds);
        Task<IReadOnlyCollection<UserGroup>> GetUserGroupsContainigPermission(string permissionId);
        Task<User> AssociateUserToUserGroup(string userId, string userGroupName);
        Task<User> DissociateUserFromUserGroup(string userId, string userGroupName);
    }

    public class AuthorizationService : IAuthorizationService
    {
        private readonly IRepository<UserGroup> _userGroupRepository;
        private readonly IReadOnlyRepository<Permission> _permissionRepository;
        private readonly IUserRepository _userRepository;

        public AuthorizationService(IRepository<UserGroup> userGroupRepository, IReadOnlyRepository<Permission> permissionRepository, IUserRepository userRepository)
        {
            _userGroupRepository = userGroupRepository;
            _permissionRepository = permissionRepository;
            _userRepository = userRepository;
        }

        public async Task<UserGroup> AddPermissionsIntoUserGroup(string userGroupName, IReadOnlyDictionary<string, int> permissions)
        {
            var errors = new string[] { };

            var userGroup = await ExceptionCatcher.ExecuteSafe<NotFoundAppException, Task<UserGroup>>(
                () => _userGroupRepository.GetByName(userGroupName),
                e => { errors = errors.Concat(e.Errors).ToArray(); });

            foreach (var kvp in permissions)
            {
                var permission = await ExceptionCatcher.ExecuteSafe<NotFoundAppException, Task<Permission>>(
                    () => _permissionRepository.GetById(kvp.Key),
                    e => { errors = errors.Concat(e.Errors).ToArray(); });

                if (permission == null)
                    continue;

                var userGroupPermission = userGroup.UserGroupPermissions?.FirstOrDefault(ugp => ugp.PermissionId == kvp.Key);

                if (userGroupPermission != null)
                {
                    userGroupPermission.PermissionOperations = kvp.Value;
                    continue;
                }

                if (userGroup.UserGroupPermissions != null)
                {
                    userGroup.UserGroupPermissions.Add(new UserGroupPermission(userGroup, permission, kvp.Value));
                    continue;
                }

                userGroup.UserGroupPermissions = new List<UserGroupPermission> { new UserGroupPermission(userGroup, permission, kvp.Value) };
            }

            if (errors.Any())
                throw new AppException(errors);

            return await _userGroupRepository.Update(userGroup);
        }

        public async Task<UserGroup> DeletePermissionsFromUserGroup(string userGroupName, IReadOnlyCollection<string> permissionsIds)
        {
            var errors = new string[] { };

            var userGroup = await ExceptionCatcher.ExecuteSafe<NotFoundAppException, Task<UserGroup>>(
                () => _userGroupRepository.GetByName(userGroupName),
                e => { errors = errors.Concat(e.Errors).ToArray(); });

            foreach (var permissionId in permissionsIds)
            {
                var permission = await ExceptionCatcher.ExecuteSafe<NotFoundAppException, Task<Permission>>(
                    () => _permissionRepository.GetById(permissionId),
                    e => { errors = errors.Concat(e.Errors).ToArray(); });

                if (permission == null)
                    continue;

                var userGroupPermissionToBeRemoved = userGroup.UserGroupPermissions?.FirstOrDefault(ugp => ugp.PermissionId == permissionId);

                if (userGroupPermissionToBeRemoved != null)
                    userGroup.UserGroupPermissions.Remove(userGroupPermissionToBeRemoved);
            }

            if (errors.Any())
                throw new AppException(errors);

            return await _userGroupRepository.Update(userGroup);
        }

        public async Task<User> AssociateUserToUserGroup(string userId, string userGroupName)
        {
            var errors = new string[] { };

            User user = await ExceptionCatcher.ExecuteSafe<NotFoundAppException, Task<User>>(
                () => _userRepository.GetById(userId),
                e => { errors = errors.Concat(e.Errors).ToArray(); });

            UserGroup userGroup = await ExceptionCatcher.ExecuteSafe<NotFoundAppException, Task<UserGroup>>(
                () => _userGroupRepository.GetByName(userGroupName),
                e => { errors = errors.Concat(e.Errors).ToArray(); });

            if (errors.Any())
                throw new NotFoundAppException(errors);

            if (!user.UserGroupUsers.Select(ugu => ugu.UserGroup.Name).Contains(userGroupName))
                user.UserGroupUsers.Add(new UserGroupUser(userGroup, user));

            return await _userRepository.Update(user, null);
        }

        public async Task<User> DissociateUserFromUserGroup(string userId, string userGroupName)
        {
            var errors = new string[] { };

            var user = await ExceptionCatcher.ExecuteSafe<NotFoundAppException, Task<User>>(
                () => _userRepository.GetById(userId),
                e => { errors = errors.Concat(e.Errors).ToArray(); });

            var userGroup = await ExceptionCatcher.ExecuteSafe<NotFoundAppException, Task<UserGroup>>(
                () => _userGroupRepository.GetByName(userGroupName),
                e => { errors = errors.Concat(e.Errors).ToArray(); });

            if (errors.Any())
                throw new NotFoundAppException(errors);

            var userGroupUserToBeRemoved = user.UserGroupUsers.FirstOrDefault(ug => ug.UserGroup.Name == userGroupName);
            if (userGroupUserToBeRemoved == null)
                throw new AppException("The user is not associated to the user group.");

            user.UserGroupUsers.Remove(userGroupUserToBeRemoved);

            return await _userRepository.Update(user, null);
        }

        public async Task<IReadOnlyCollection<UserGroup>> GetUserGroupsContainigPermission(string permissionId)
        {
            var permission = await _permissionRepository.GetById(permissionId);

            return permission.UserGroupPermissions
                .Select(ugp => ugp.UserGroup)
                .ToArray();
        }
    }
}
