using Microsoft.EntityFrameworkCore;
using Identidade.Dominio.Helpers;
using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Identidade.Dominio.Servicos;

namespace Identidade.Dominio.Repositorios
{
    public class UserGroupRepository : UserReadOnlyRepository, IRepository<UserGroup>, IUserGroupRepository
    {
        private readonly IUpdateConcurrencyResolver _updateConcurrencyResolver;
        private readonly IIdGenerator _idGenerator;

        public UserGroupRepository(IARCDbContext arcDbContext, IUpdateConcurrencyResolver updateConcurrencyResolver, IIdGenerator idGenerator)
            : base(arcDbContext, updateConcurrencyResolver)
        {
            _updateConcurrencyResolver = updateConcurrencyResolver ?? throw new ArgumentNullException(nameof(updateConcurrencyResolver));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
        }

        public async Task<UserGroup> Create(UserGroup userGroup)
        {
            if (userGroup == null)
                throw new ArgumentNullException(nameof(userGroup));

            if (string.IsNullOrWhiteSpace(userGroup.Name))
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
                userGroupUser.UserGroupId = userGroup.Id;
            }

            var entityEntry = await _arcDbContext.AddAsync(userGroup);
            await _arcDbContext.SaveChangesAsync();

            return entityEntry.Entity;
        }

        public async Task<UserGroup> Update(UserGroup userGroup)
        {
            if (userGroup == null)
                throw new ArgumentNullException(nameof(userGroup));

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

        public new async Task<UserGroup> GetById(string userGroupId)
        {
            if (string.IsNullOrWhiteSpace(userGroupId))
                throw new ArgumentException("UserGroupId must be provided", nameof(userGroupId));

            var userGroup = await GetAllUserGroups()
                .SingleOrDefaultAsync(ug => ug.Id == userGroupId);

            if (userGroup == null)
                throw new NotFoundAppException("user group", "ID", userGroupId);

            return userGroup;
        }

        async Task<IReadOnlyDictionary<string, UserGroup>> IReadOnlyRepository<UserGroup>.GetByIds(string[] ids)
        {
            if (ids is null || ids.Length == 0)
                return new Dictionary<string, UserGroup>(StringComparer.OrdinalIgnoreCase);

            var normalized = ids
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToArray();

            if (normalized.Length == 0)
                return new Dictionary<string, UserGroup>(StringComparer.OrdinalIgnoreCase);

            var userGroups = await GetAllUserGroups()
                .Where(ug => normalized.Contains(ug.Id))
                .ToArrayAsync();

            return userGroups.ToDictionary(ug => ug.Id, ug => ug, StringComparer.OrdinalIgnoreCase);
        }

        public new async Task<UserGroup> GetByName(string userGroupName)
        {
            if (string.IsNullOrWhiteSpace(userGroupName))
                throw new ArgumentException("UserGroupName must be provided", nameof(userGroupName));

            var normalized = userGroupName.Trim();

            var userGroup = await GetAllUserGroups()
                .SingleOrDefaultAsync(ug => ug.Name == normalized);

            if (userGroup == null)
                throw new NotFoundAppException("user group", "name", userGroupName);

            return userGroup;
        }

        public new async Task<IReadOnlyCollection<UserGroup>> GetAll() =>
            await GetAllUserGroups().ToArrayAsync();

        Task<IReadOnlyCollection<UserGroup>> IReadOnlyRepository<UserGroup>.GetAll(int? page, int? pageSize) =>
            GetAll(page, pageSize);

        public new async Task<IReadOnlyCollection<UserGroup>> GetAll(int? page, int? pageSize)
        {
            if (!page.HasValue && !pageSize.HasValue)
                return await GetAll();

            var pagination = new OpcoesPaginacao(page, pageSize);
            return await GetAllUserGroups()
                .Skip(pagination.Skip)
                .Take(pagination.TamanhoPagina)
                .ToArrayAsync();
        }

        public async Task<UserGroup[]> GetUserGroups(string[] userGroupIds)
        {
            if (userGroupIds == null || userGroupIds.Length == 0)
                return [];

            var normalizedUserGroupIds = userGroupIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.ToUpperInvariant())
                .ToHashSet();

            if (normalizedUserGroupIds.Count == 0)
                return [];

            return await _arcDbContext.UserGroups
                .AsNoTracking()
                .Where(ug => normalizedUserGroupIds.Contains(ug.Id.ToUpperInvariant()))
                .ToArrayAsync();
        }

        public async Task<User[]> GetUsers(string[] userIds)
        {
            if (userIds == null || userIds.Length == 0)
                return [];

            var normalizedUserIds = userIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.ToUpperInvariant())
                .ToHashSet();

            if (normalizedUserIds.Count == 0)
                return [];

            return await _arcDbContext.Users
                .AsNoTracking()
                .AsSplitQuery()
                .Where(u => normalizedUserIds.Contains(u.Id.ToUpperInvariant()))
                .Include(u => u.UserGroupUsers)
                .ThenInclude(ugu => ugu.UserGroup)
                .Include(u => u.UserSubstitutions)
                .ThenInclude(usu => usu.SubstituteUser)
                .ToArrayAsync();
        }

        private IQueryable<UserGroup> GetAllUserGroups() =>
            AddUserGroupsRelatedData(_arcDbContext.UserGroups);

        private IQueryable<UserGroup> AddUserGroupsRelatedData(IQueryable<UserGroup> userGroups)
        {
            return userGroups
                .AsNoTracking()
                .AsSplitQuery()
                .Include(ug => ug.UserGroupPermissions)
                .ThenInclude(ugp => ugp.Permission)
                .Include(ug => ug.UserGroupUsers)
                .ThenInclude(ugu => ugu.User);
        }
    }
}
