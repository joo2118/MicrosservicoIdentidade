using Xunit;
using NSubstitute;
using Microsoft.Extensions.Configuration;
using Identidade.Infraestrutura.Helpers;
using Microsoft.EntityFrameworkCore;
using Identidade.Infraestrutura.Data;
using Microsoft.Data.SqlClient;

namespace Identidade.UnitTests.Infraestrutura.Helpers
{
    public class DatabaseConnectionUserModifierTests
    {
        [Fact]
        public void ModifyConnection_SetsApplicationNameInConnectionString_WhenUserIdIsValid()
        {
            var config = Substitute.For<IConfiguration>();

            var connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=TestDb;Integrated Security=true;";
            var connection = new SqlConnection(connectionString);

            var options = new DbContextOptionsBuilder<ARCDbContext>()
                .UseSqlServer(connection)
                .Options;
            var context = new ARCDbContext(options);

            context.Database.EnsureCreated();

            config.GetConnectionString("DefaultConnection").Returns(connectionString);
            

            var modifier = new DatabaseConnectionUserModifier(config);

            modifier.ModifyConnection(context, "user123");

            Assert.NotNull(context.Database);
            Assert.NotNull(context.Database.GetDbConnection().ConnectionString);

            var modifiedConnectionString = context.Database.GetDbConnection().ConnectionString;
            Assert.Contains("Application Name=\"Identidade Usuario (user123)\"", modifiedConnectionString);

            connection.Close();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ModifyConnection_DoesNothing_WhenUserIdIsNullOrEmpty(string userId)
        {
            var config = Substitute.For<IConfiguration>();
            var connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=TestDb;Integrated Security=true;";
            var connection = new SqlConnection(connectionString);

            var options = new DbContextOptionsBuilder<ARCDbContext>()
                .UseSqlServer(connection)
                .Options;
            var context = new ARCDbContext(options);

            context.Database.EnsureCreated();

            config.GetConnectionString("DefaultConnection").Returns("Server=.;Database=TestDb;User Id=test;Password=test;");

            var modifier = new DatabaseConnectionUserModifier(config);

            modifier.ModifyConnection(context, userId);

            Assert.Equal(context.Database.GetDbConnection().ConnectionString, connectionString);
            connection.Close();
        }

        [Fact]
        public void ModifyConnection_DoesNothing_WhenConnectionStringIsNullOrEmpty()
        {
            var config = Substitute.For<IConfiguration>();
            var connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=TestDb;Integrated Security=true;";
            var connection = new SqlConnection(connectionString);

            var options = new DbContextOptionsBuilder<ARCDbContext>()
                .UseSqlServer(connection)
                .Options;
            var context = new ARCDbContext(options);

            context.Database.EnsureCreated();

            config.GetConnectionString("DefaultConnection").Returns((string)null);

            var modifier = new DatabaseConnectionUserModifier(config);

            modifier.ModifyConnection(context, "user123");

            Assert.Equal(context.Database.GetDbConnection().ConnectionString, connectionString);
            connection.Close();
        }
    }
}