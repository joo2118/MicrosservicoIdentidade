using Microsoft.EntityFrameworkCore;
using Identidade.Dominio.Helpers;
using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Identidade.Dominio.Repositorios
{
    public class PermissionRepository : UserReadOnlyRepository, IReadOnlyRepository<Permission>
    {
        public PermissionRepository(IARCDbContext arcDbContext, IUpdateConcurrencyResolver updateConcurrencyResolver)
            : base(arcDbContext, updateConcurrencyResolver) { }

        public new async Task<Permission> GetById(string permissionId)
        {
            if (string.IsNullOrWhiteSpace(permissionId))
                throw new ArgumentException("PermissionId must be provided", nameof(permissionId));

            var permission = await QueryWithRelatedData()
                .SingleOrDefaultAsync(p => p.Id == permissionId);

            if (permission == null)
                throw new NotFoundAppException("permission", "ID", permissionId);

            return permission;
        }

        async Task<IReadOnlyDictionary<string, Permission>> IReadOnlyRepository<Permission>.GetByIds(string[] ids)
        {
            if (ids is null || ids.Length == 0)
                return new Dictionary<string, Permission>(StringComparer.OrdinalIgnoreCase);

            var normalized = ids
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToArray();

            if (normalized.Length == 0)
                return new Dictionary<string, Permission>(StringComparer.OrdinalIgnoreCase);

            var perms = await QueryWithRelatedData()
                .Where(p => normalized.Contains(p.Id))
                .ToArrayAsync();

            return perms.ToDictionary(p => p.Id, p => p, StringComparer.OrdinalIgnoreCase);
        }

        public new async Task<Permission> GetByName(string permissionName)
        {
            if (string.IsNullOrWhiteSpace(permissionName))
                throw new ArgumentException("PermissionName must be provided", nameof(permissionName));

            var normalized = permissionName.Trim();

            var permission = await QueryWithRelatedData()
                .SingleOrDefaultAsync(p => p.Name == normalized);

            if (permission == null)
                throw new NotFoundAppException("permission", "name", permissionName);

            return permission;
        }

        public new async Task<IReadOnlyCollection<Permission>> GetAll() =>
            await QueryWithRelatedData().ToArrayAsync();

        public new async Task<IReadOnlyCollection<Permission>> GetAll(int? page, int? pageSize)
        {
            if (!page.HasValue && !pageSize.HasValue)
                return await GetAll();

            var pagination = new OpcoesPaginacao(page, pageSize);
            return await QueryWithRelatedData()
                .Skip(pagination.Skip)
                .Take(pagination.TamanhoPagina)
                .ToArrayAsync();
        }

        Task<IReadOnlyCollection<Permission>> IReadOnlyRepository<Permission>.GetAll(int? page, int? pageSize) =>
            GetAll(page, pageSize);

        private IQueryable<Permission> QueryWithRelatedData()
        {
            return _arcDbContext.Permissions
                .AsNoTracking()
                .AsSplitQuery()
                .Include(p => p.UserGroupPermissions);
        }
    }
}
