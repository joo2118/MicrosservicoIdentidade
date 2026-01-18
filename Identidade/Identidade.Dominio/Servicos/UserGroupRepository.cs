using Microsoft.EntityFrameworkCore;
using Identidade.Dominio.Helpers;
using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Identidade.Dominio.Servicos
{
    public class UserGroupRepository : UserReadOnlyRepository, IRepository<UserGroup>, IUserGroupRepository
    {
        private readonly IUpdateConcurrencyResolver _updateConcurrencyResolver;
        private readonly IIdGenerator _idGenerator;

        public UserGroupRepository(IARCDbContext arcDbContext, IUpdateConcurrencyResolver updateConcurrencyResolver, IIdGenerator idGenerator)
            : base (arcDbContext, updateConcurrencyResolver)
        {
            _updateConcurrencyResolver = updateConcurrencyResolver;
            _idGenerator = idGenerator;
        }

        public async Task<UserGroup> Create(UserGroup userGroup)
        {
            if (userGroup.Name == null || userGroup.Name == string.Empty)
                throw new AppException("The group name cannot be null.");

            userGroup.CreatedAt = DateTime.UtcNow;
            userGroup.LastUpdatedAt = userGroup.CreatedAt;

            userGroup.Id = _idGenerator.GenerateId("UGR", userGroup.Id);

            var userGroupPermissions = userGroup.UserGroupPermissions ?? Enumerable.Empty<UserGroupPermission>();
            foreach (var userGroupPermission in userGroupPermissions)
            {
                userGroupPermission.UserGroup = userGroup;
                userGroupPermission.UserGroupId = userGroup.Id;
            }

            var userGroupUsers = userGroup.UserGroupUsers ?? Enumerable.Empty<UserGroupUser>();
            foreach (var userGroupUser in userGroupUsers)
            {
                userGroupUser.UserGroup = userGroup;
                userGroupUser.UserId = userGroup.Id;
            }

            var entityEntry = await _arcDbContext.AddAsync(userGroup);
            await _arcDbContext.SaveChangesAsync();

            return entityEntry.Entity;
        }

        public async Task<UserGroup> Update(UserGroup userGroup)
        {
            userGroup.LastUpdatedAt = DateTime.UtcNow;

            var entityEntry = _arcDbContext.UserGroups.Update(userGroup);
            await _updateConcurrencyResolver.SaveChangesSafe();

            return entityEntry.Entity;
        }

        public async Task<bool> Remove(string entityId)
        {
            var userGroup = await GetById(entityId);

            _arcDbContext.UserGroups.Remove(userGroup);
            var result = await _arcDbContext.SaveChangesAsync();

            return result != 0;
        }

        public async Task<string> RemoveByName(string userGroupName)
        {
            var userGroup = await GetByName(userGroupName);

            _arcDbContext.UserGroups.Remove(userGroup);
            await _arcDbContext.SaveChangesAsync();

            return userGroup.Id;
        }

        public async Task<UserGroup> GetById(string userGroupId)
        {
            var userGroup = await GetAllUserGroups()
                .SingleOrDefaultAsync(ug => ug.Id == userGroupId);

            if (userGroup == null)
                throw new NotFoundAppException("user group", "ID", userGroupId);

            return userGroup;
        }

        public async Task<UserGroup> GetByName(string userGroupName)
        {
            var user = await GetAllUserGroups()
                .SingleOrDefaultAsync(ug => ug.Name == userGroupName);

            if (user == null)
                throw new NotFoundAppException("user group", "name", userGroupName);

            return user;
        }

        public async Task<IReadOnlyCollection<UserGroup>> GetAll() =>
            await GetAllUserGroups()
                .ToArrayAsync();

        public async Task<UserGroup[]> GetUserGroups(string[] userGroupIds)
        {
            if (userGroupIds == null || userGroupIds.Length == 0)
                return [];

            var normalizedUserGroupIds = userGroupIds.Select(id => id.ToUpperInvariant()).ToHashSet();
            var userGroups = await _arcDbContext.UserGroups.ToArrayAsync();

            return userGroups
                .Where(ug => normalizedUserGroupIds.Contains(ug.Id.ToUpperInvariant()))
                .ToArray();
        }

        public async Task<User[]> GetUsers(string[] userIds)
        {
            if (userIds == null || userIds.Length == 0)
                return [];

            var normalizedUserIds = userIds.Select(id => id.ToUpperInvariant()).ToHashSet();

            var users = await _arcDbContext.Users
                .Include(u => u.UserGroupUsers)
                .ThenInclude(ugu => ugu.UserGroup)
                .Include(u => u.UserSubstitutions)
                .ThenInclude(usu => usu.SubstituteUser)
                .ToArrayAsync();

            return users
                .Where(us => normalizedUserIds.Contains(us.Id.ToUpperInvariant()))
                .ToArray();
        }

        private IQueryable<UserGroup> GetAllUserGroups() =>
                AddUserGroupsRelatedData(_arcDbContext.UserGroups);

        private IQueryable<UserGroup> AddUserGroupsRelatedData(IQueryable<UserGroup> userGroups)
        {
            return userGroups
                .Include(ug => ug.UserGroupPermissions)
                .ThenInclude(ugp => ugp.Permission)
                .Include(ug => ug.UserGroupUsers)
                .ThenInclude(ugu => ugu.User);
        }
    }
}
