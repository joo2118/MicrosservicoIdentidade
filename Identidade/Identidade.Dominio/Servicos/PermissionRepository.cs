using Microsoft.EntityFrameworkCore;
using Identidade.Dominio.Helpers;
using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Modelos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Identidade.Dominio.Servicos
{
    public class PermissionRepository : UserReadOnlyRepository, IReadOnlyRepository<Permission>
    {
        public PermissionRepository(IARCDbContext arcDbContext, IUpdateConcurrencyResolver updateConcurrencyResolver)
            : base(arcDbContext, updateConcurrencyResolver) { }
        
        public async Task<Permission> GetById(string permissionId)
        {
            var permission = await _arcDbContext.Permissions
                .Include(p => p.UserGroupPermissions)
                .SingleOrDefaultAsync(p => p.Id == permissionId);
            
            if (permission == null)
                throw new NotFoundAppException("permission", "ID", permissionId);

            return permission;
        }

        public async Task<Permission> GetByName(string permissionName)
        {
            var permission = await _arcDbContext.Permissions
                .Include(p => p.UserGroupPermissions)
                .SingleOrDefaultAsync(p => p.Name == permissionName);

            if (permission == null)
                throw new NotFoundAppException("permission", "name", permissionName);

            return permission;
        }

        public async Task<IReadOnlyCollection<Permission>> GetAll() =>
            await _arcDbContext.Permissions
                .Include(p => p.UserGroupPermissions)
                .ToArrayAsync();
    }
}
