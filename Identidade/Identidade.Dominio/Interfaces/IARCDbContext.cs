using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Identidade.Dominio.Modelos;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Identidade.Dominio.Interfaces
{
    public interface IARCDbContext
    {
        DbSet<User> Users { get; set; }
        DbSet<UserGroup> UserGroups { get; set; }
        DbSet<Permission> Permissions { get; set; }
        DbSet<UserSubstitution> UserSubstitutions { get; set; }
        DbSet<MessageInformation> ConsumedMessages { get; set; }
        DatabaseFacade Database { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
        ValueTask<EntityEntry<User>> AddAsync(User user, CancellationToken cancellationToken = default(CancellationToken));
        ValueTask<EntityEntry<UserGroup>> AddAsync(UserGroup userGroup, CancellationToken cancellationToken = default(CancellationToken));

        int CreateUpdateArcUser(string sql, List<SqlParameter> parameters);
        int RemoveArcUser(string sql, List<SqlParameter> parameters);
        void BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();
        EntityEntry<User> Update(User user);
        EntityEntry<User> Remove(User user);
        Task AddRangeAsync(IEnumerable<object> entities, CancellationToken cancellationToken = default(CancellationToken));
        void RemoveRange(IEnumerable<object> entities);
    }
}
