using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Identidade.Dominio.Interfaces;

namespace Identidade.Infraestrutura.Helpers
{
    public interface IDatabaseConnectionUserModifier
    {
        void ModifyConnection(IARCDbContext context, string userId);
    }

    public class DatabaseConnectionUserModifier : IDatabaseConnectionUserModifier
    {
        private readonly string _connectionString;

        public DatabaseConnectionUserModifier(IConfiguration configs)
        {
            _connectionString = configs.GetConnectionString("DefaultConnection");
        }

        public void ModifyConnection(IARCDbContext context, string userId)
        {
            if (string.IsNullOrEmpty(_connectionString) || string.IsNullOrEmpty(userId))
                return;

            var csb = new SqlConnectionStringBuilder
            {
                ConnectionString = _connectionString,
                ApplicationName = $"Identidade Usuario ({userId})",
            };

            context.Database.GetDbConnection().ConnectionString = csb.ConnectionString;
        }
    }
}
