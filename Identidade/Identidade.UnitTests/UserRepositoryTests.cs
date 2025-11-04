using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Identidade.Dominio.Helpers;
using Identidade.Dominio.Modelos;
using Identidade.Dominio.Servicos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Identidade.Infraestrutura.Data;
using Identidade.UnitTests.Helpers;
using Xunit;

namespace Identidade.UnitTests
{
    public class UserRepositoryTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<ARCDbContext> _options;
        private readonly string _defaultConnection = "Data Source=:memory:";

        public UserRepositoryTests()
        {
            _connection = new SqliteConnection(_defaultConnection);
            _connection.Open();

            _options = new DbContextOptionsBuilder<ARCDbContext>()
                .UseSqlite(_connection)
                .Options;

            using (var context = new ARCDbContext(_options))
            {
                context.Database.EnsureCreated();
            }
        }
        [Fact]
        public void Create_NewUser_NullOrEmpty()
        {
            using (var context = new ARCDbContext(_options))
            {
                var userRepository = new UserRepositoryBuilder()
                    .WithARCDbContext(context)
                    .Build();

                var userNullEmpty = Assert.ThrowsAsync<Exception>(async () => await userRepository.Create(null, "password"));

                Assert.Contains("The user can not be null", userNullEmpty.Exception.Message);
            }
        }
        [Fact]
        public async Task Create_NewUser_Success()
        {
            DateTime before;
            DateTime after;

            using (var context = new ARCDbContext(_options))
            {
                var mqIdGenerator = new Mock<IIdGenerator>();
                mqIdGenerator.Setup(ig => ig.GenerateId("USR", It.IsAny<string>()))
                    .Returns("USR_GUID");

                var mqUserValidator = new Mock<IUserValidator>();
                var mqPasswordValidator = new Mock<IPasswordValidator>();

                var userRepository = new UserRepositoryBuilder()
                    .WithIdGenerator(mqIdGenerator.Object)
                    .WithUserNameValidator(mqUserValidator.Object)
                    .WithPasswordValidator(mqPasswordValidator.Object)
                    .WithARCDbContext(context)
                    .WithUpdateConcurrencyResolver(new UpdateConcurrencyResolver(context))
                    .Build();

                var user = new User
                {
                    Name = "New User",
                    UserName = "new.user",
                    Email = "new.user@email.com"
                };

                before = DateTime.UtcNow;
                var createdUser = await userRepository.Create(user, "password");
                after = DateTime.UtcNow;

                mqUserValidator.Verify(uv => uv.Validate(user.UserName), Times.Once);
                mqPasswordValidator.Verify(pv => pv.Validate("password"), Times.Once);

                Assert.Equal("New User", createdUser.Name);
                Assert.Equal("new.user", createdUser.UserName);
                Assert.Equal("new.user@email.com", createdUser.Email);
                Assert.Equal("USR_GUID", createdUser.Id);
                Assert.Equal("EncriptedPassword", createdUser.PasswordHash);
                Assert.Equal("EncriptedPassword", createdUser.PasswordHistory);
                Assert.Equal(createdUser.CreatedAt, createdUser.LastUpdatedAt);
                Assert.True(before < createdUser.CreatedAt && createdUser.CreatedAt < after);
            }

            using (var context = new ARCDbContext(_options))
            {
                var user = await context.Users.FindAsync("USR_GUID");

                Assert.Equal("New User", user.Name);
                Assert.Equal("new.user", user.UserName);
                Assert.Equal("new.user@email.com", user.Email);
                Assert.Equal("USR_GUID", user.Id);
                Assert.Equal("EncriptedPassword", user.PasswordHash);
                Assert.Equal("EncriptedPassword", user.PasswordHistory);
                Assert.Equal(user.CreatedAt, user.LastUpdatedAt);
                Assert.True(before < user.CreatedAt && user.CreatedAt < after);
            }
        }

        [Fact]
        public async Task Update_Password_PasswordHistoryUpdated()
        {
            using (var context = new ARCDbContext(_options))
            {
                var user = new User { Id = "USR_GUID", UserName = "user", PasswordHash = "EncriptedPassword" };

                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();
            }

            using (var context = new ARCDbContext(_options))
            {
                var userRepository = new UserRepositoryBuilder()
                    .WithARCDbContext(context)
                    .WithUpdateConcurrencyResolver(new UpdateConcurrencyResolver(context))
                    .Build();

                var user = new User { Id = "USR_GUID", UserName = "user", PasswordHash = "NewEncriptedPassword" };

                var updatedUser = await userRepository.Update(user, "EncriptedPassword");

                Assert.Equal("user", updatedUser.UserName);
                Assert.Equal("USR_GUID", updatedUser.Id);
                Assert.Equal("NewEncriptedPassword", updatedUser.PasswordHash);
                Assert.Equal("NewEncriptedPassword", updatedUser.PasswordHistory);
            }

            using (var context = new ARCDbContext(_options))
            {
                var updatedUser = await context.Users.FindAsync("USR_GUID");

                Assert.Equal("user", updatedUser.UserName);
                Assert.Equal("USR_GUID", updatedUser.Id);
                Assert.Equal("NewEncriptedPassword", updatedUser.PasswordHash);
                Assert.Equal("NewEncriptedPassword", updatedUser.PasswordHistory);
            }
        }

        [Fact]
        public async Task Remove_ExistentUser_Success()
        {
            using (var context = new ARCDbContext(_options))
            {
                var user = new User { Id = "USR_GUID", UserName = "user" };

                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();
            }

            using (var context = new ARCDbContext(_options))
            {
                var userRepository = new UserRepositoryBuilder()
                    .WithARCDbContext(context)
                    .Build();

                await userRepository.Remove("USR_GUID");
            }

            using (var context = new ARCDbContext(_options))
            {
                var removedUser = await context.Users.FindAsync("USR_GUID");

                Assert.Null(removedUser);
            }
        }

        [Fact]
        public async Task Remove_InexistentUser_ThrowsException()
        {
            using (var context = new ARCDbContext(_options))
            {
                var userRepository = new UserRepositoryBuilder()
                    .WithARCDbContext(context)
                    .Build();

                await Assert.ThrowsAsync<NotFoundAppException>(() => userRepository.Remove("USR_GUID"));
            }
        }

        [Fact]
        public async Task GetById_IncludeUserGroups()
        {
            using (var context = new ARCDbContext(_options))
            {
                User user = CreateUserWithUserGroups();

                await context.AddRangeAsync(user);
                await context.SaveChangesAsync();
            }

            using (var context = new ARCDbContext(_options))
            {
                var userRepository = new UserRepositoryBuilder()
                    .WithARCDbContext(context)
                    .Build();

                var user = await userRepository.GetById("USR_GUID");

                Assert.Equal("USR_GUID", user.Id);
                Assert.Equal("user", user.UserName);
                Assert.Equal(2, user.UserGroupUsers.Count);
                Assert.Equal(new[] {"UGR_GUID_1", "UGR_GUID_2"}, user.UserGroupUsers.Select(ugu => ugu.UserGroupId).OrderBy(id => id));
                Assert.Equal(new[] {"User Group 1", "User Group 2"}, user.UserGroupUsers.Select(ugu => ugu.UserGroup.Name).OrderBy(name => name));
            }
        }

        [Fact]
        public async Task GetByName_IncludeUserGroups()
        {
            using (var context = new ARCDbContext(_options))
            {
                var user = CreateUserWithUserGroups();

                await context.AddRangeAsync(user);
                await context.SaveChangesAsync();
            }

            using (var context = new ARCDbContext(_options))
            {
                var userRepository = new UserRepositoryBuilder()
                    .WithARCDbContext(context)
                    .Build();

                var user = await userRepository.GetByName("user");

                Assert.Equal("USR_GUID", user.Id);
                Assert.Equal("user", user.UserName);
                Assert.Equal(2, user.UserGroupUsers.Count);
                Assert.Equal(new[] {"UGR_GUID_1", "UGR_GUID_2"}, user.UserGroupUsers.Select(ugu => ugu.UserGroupId).OrderBy(id => id));
                Assert.Equal(new[] {"User Group 1", "User Group 2"}, user.UserGroupUsers.Select(ugu => ugu.UserGroup.Name).OrderBy(name => name));
            }
        }
        [Fact]
        public void GetUserByName_Exception()
        {
            using (var context = new ARCDbContext(_options))
            {
                var userRepository = new UserRepositoryBuilder()
                    .WithARCDbContext(context)
                    .Build();

                var user = new User { Name = "New Group" };

                var userByNameException = Assert.ThrowsAsync<Exception>(async () => await userRepository.GetByName(user.Name));

                Assert.Contains("There's no user with the user name 'New Group' on the database.", userByNameException.Exception.Message);
            }
        }
        [Fact]
        public async Task GetById_IncludeSubstituteUsers()
        {
            using (var context = new ARCDbContext(_options))
            {
                var user = new User { Id = "USR_GUID", UserName = "user" };
                
                var substituteUser1 = new User { Id = "USR_GUID_1", Name = "Substitute User 1" };
                var substituteUser2 = new User { Id = "USR_GUID_2", Name = "Substitute User 2" };

                await context.AddRangeAsync(
                    user,
                    new UserSubstitution(user, substituteUser1),
                    new UserSubstitution(user, substituteUser2));

                await context.SaveChangesAsync();
            }

            using (var context = new ARCDbContext(_options))
            {
                var userRepository = new UserRepositoryBuilder()
                    .WithARCDbContext(context)
                    .Build();

                var user = await userRepository.GetById("USR_GUID");

                Assert.Equal("USR_GUID", user.Id);
                Assert.Equal("user", user.UserName);
                Assert.Equal(2, user.UserSubstitutions.Count);
                Assert.Equal(new[] {"USR_GUID_1", "USR_GUID_2"}, user.UserSubstitutions.Select(ugu => ugu.SubstituteUserId).OrderBy(id => id));
                Assert.Equal(new[] {"Substitute User 1", "Substitute User 2"}, user.UserSubstitutions.Select(ugu => ugu.SubstituteUser.Name).OrderBy(name => name));
            }
        }

        [Fact]
        public async Task GetByName_IncludeSubstituteUsers()
        {
            using (var context = new ARCDbContext(_options))
            {
                var user = new User { Id = "USR_GUID", UserName = "user" };
                
                var substituteUser1 = new User { Id = "USR_GUID_1", Name = "Substitute User 1" };
                var substituteUser2 = new User { Id = "USR_GUID_2", Name = "Substitute User 2" };

                await context.AddRangeAsync(
                    user,
                    new UserSubstitution(user, substituteUser1),
                    new UserSubstitution(user, substituteUser2));

                await context.SaveChangesAsync();
            }

            using (var context = new ARCDbContext(_options))
            {
                var userRepository = new UserRepositoryBuilder()
                    .WithARCDbContext(context)
                    .Build();

                var user = await userRepository.GetByName("user");

                Assert.Equal("USR_GUID", user.Id);
                Assert.Equal("user", user.UserName);
                Assert.Equal(2, user.UserSubstitutions.Count);
                Assert.Equal(new[] {"USR_GUID_1", "USR_GUID_2"}, user.UserSubstitutions.Select(ugu => ugu.SubstituteUserId).OrderBy(id => id));
                Assert.Equal(new[] {"Substitute User 1", "Substitute User 2"}, user.UserSubstitutions.Select(ugu => ugu.SubstituteUser.Name).OrderBy(name => name));
            }
        }

        private static User CreateUserWithUserGroups()
        {
            var user = new User { Id = "USR_GUID", UserName = "user" };

            var userGroup1 = new UserGroup { Id = "UGR_GUID_1", Name = "User Group 1" };
            var userGroup2 = new UserGroup { Id = "UGR_GUID_2", Name = "User Group 2" };

            user.UserGroupUsers.Add(new UserGroupUser(userGroup1, user));
            user.UserGroupUsers.Add(new UserGroupUser(userGroup2, user));

            return user;
        }

        [Fact]
        public async Task GetAllUsers()
        {
            using (var context = new ARCDbContext(_options))
            {
                var user1 = new User { Id = "USR_GUID1", UserName = "user1" };

                await context.Users.AddAsync(user1);
                await context.SaveChangesAsync();

                var user2 = new User { Id = "USR_GUID2", UserName = "user2" };

                await context.Users.AddAsync(user2);
                await context.SaveChangesAsync();
            }

            using (var context = new ARCDbContext(_options))
            {
                var userRepository = new UserRepositoryBuilder()
                    .WithARCDbContext(context)
                    .Build();

                var allUsers = await userRepository.GetAll();

                foreach (var user in allUsers)
                {
                    if (user.Id == "USR_00000000-0000-0000-0000-000000000000")
                    {
                        await userRepository.Remove(user.Id);
                    }
                }
                allUsers = await userRepository.GetAll();
                Assert.NotNull(allUsers);
                Assert.Equal(2, allUsers.Count);
            }
        }

        public void Dispose()
        {
            _connection.Close();
        }
    }
}
