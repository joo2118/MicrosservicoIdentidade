using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Modelos;
using Identidade.Dominio.Modelos;

namespace Identidade.Infraestrutura.Data
{
    public class ARCDbContext : IdentityDbContext<User>, IARCDbContext
    {
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserSubstitution> UserSubstitutions { get; set; }
        public DbSet<MessageInformation> ConsumedMessages { get; set; }

        public ARCDbContext(DbContextOptions<ARCDbContext> options)
            : base(options) {}

        public ValueTask<EntityEntry<User>> AddAsync(User user, CancellationToken cancellationToken = default(CancellationToken)) =>
            base.AddAsync(user, cancellationToken);

        public ValueTask<EntityEntry<UserGroup>> AddAsync(UserGroup userGroup, CancellationToken cancellationToken = default(CancellationToken)) =>
            base.AddAsync(userGroup, cancellationToken);

        public int CreateUpdateArcUser(string sql, List<SqlParameter> parameters) =>
            base.Database.ExecuteSqlRaw(sql, parameters);

        public int RemoveArcUser(string sql, List<SqlParameter> parameters) =>
            base.Database.ExecuteSqlRaw(sql, parameters);

        public void BeginTransaction() => base.Database.BeginTransaction();

        public void CommitTransaction() => base.Database.CommitTransaction();

        public void RollbackTransaction() => base.Database.RollbackTransaction();

        public EntityEntry<User> Remove(User user) =>
            base.Remove(user);

        public EntityEntry<User> Update(User user) =>
            base.Update(user);

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasDefaultSchema(Constants.cst_SchemaIdentity);

            builder.Entity<UserGroupUser>().Property(e => e.UserGroupId).HasConversion(p => p, s => s.ToUpper());

            builder.Entity<UserGroup>().Property(e => e.Id).HasConversion(p => p, s => s.ToUpper());

            builder.Entity<UserGroupUser>()
                .HasKey(item => new {item.UserGroupId, item.UserId});

            builder.Entity<UserGroupPermission>()
                .HasKey(item => new {item.UserGroupId, item.PermissionId});
            
            builder.Entity<UserSubstitution>()
                .HasKey(item => new { item.UserId, item.SubstituteUserId });

            builder.Entity<UserGroup>()
                .HasIndex(u => u.Name)
                .IsUnique();

            builder.Entity<User>()
                .HasIndex(u => u.UserName)
                .IsUnique();
            
            builder.Entity<User>()
                .HasMany(user => user.UserSubstitutions)
                .WithOne(usu => usu.User)
                .OnDelete(DeleteBehavior.Restrict);           

            builder.Entity<Permission>().HasData(PermissionRecord.All);
        }
    }
}
