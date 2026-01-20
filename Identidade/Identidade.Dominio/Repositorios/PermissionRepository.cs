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

        public async Task<Permission> GetById(string permissionId)
        {
            if (string.IsNullOrWhiteSpace(permissionId))
                throw new ArgumentException("PermissionId must be provided", nameof(permissionId));

            var permission = await QueryWithRelatedData()
                .SingleOrDefaultAsync(p => p.Id == permissionId);

            if (permission == null)
                throw new NotFoundAppException("permission", "ID", permissionId);

            return permission;
        }

        public async Task<Permission> GetByName(string permissionName)
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

        public async Task<IReadOnlyCollection<Permission>> GetAll() =>
            await QueryWithRelatedData().ToArrayAsync();

        private IQueryable<Permission> QueryWithRelatedData()
        {
            return _arcDbContext.Permissions
                .AsNoTracking()
                .AsSplitQuery()
                .Include(p => p.UserGroupPermissions);
        }
    }
}
