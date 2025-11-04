using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Identidade.Dominio.Helpers;
using Identidade.Dominio.Modelos;
using Identidade.Dominio.Servicos;
using Identidade.Infraestrutura.Data;
using Identidade.UnitTests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Identidade.UnitTests
{
    public class UserGroupRepositoryTests
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<ARCDbContext> _options;
        public UserGroupRepositoryTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
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
        public async Task UserGroupAddAsync()
        {
            using (var context = new ARCDbContext(_options))
            {
                var userGroup = new UserGroup { Id = "USR_GUID", Name = "userGroupMaster"};

                await context.UserGroups.AddAsync(userGroup);
                await context.SaveChangesAsync();

                Assert.Equal("USR_GUID", userGroup.Id);
                Assert.Equal("userGroupMaster", userGroup.Name);
            }
        }

        [Fact]
        public async Task UserGroupAddAsyncArgumentNullException()
        {
            using (var context = new ARCDbContext(_options))
            {
                await Assert.ThrowsAsync<ArgumentNullException>(async () => await context.UserGroups.AddAsync(null));
            }
        }

        [Fact]
        public async Task UserGroupAddAsyncInvalidOperationException()
        {
            var userGroup = new UserGroup();
            using (var context = new ARCDbContext(_options))
            {
                await Assert.ThrowsAsync<InvalidOperationException>(async () => await context.UserGroups.AddAsync(userGroup));
            }
        }

        [Fact]
        public async Task Create_NewUserGroup_Success()
        {
            DateTime before;
            DateTime after;

            using (var context = new ARCDbContext(_options))
            {
                var mqIdGenerator = new Mock<IIdGenerator>();
                mqIdGenerator.Setup(ig => ig.GenerateId("UGR", It.IsAny<string>()))
                    .Returns("UGR_GUID");

                var userGroupRepository = new UserGroupRepositoryBuilder()
                    .WithARCDbContext(context)
                    .WithIdGenerator(mqIdGenerator.Object)
                    .Build();

                var user = new User { Name = "New User", UserName = "new.user", Email = "new.user@email.com"};
                var userGroup2 = new UserGroup { Name = "New Group"};
                var userGroupUser = new UserGroupUser(userGroup2, user);

                var permission = new Permission { Id = "", Name = "" };

                var userGroupPermission = new UserGroupPermission(userGroup2, permission, 1);

                var userGroup = new UserGroup
                {
                    Name = "New Group",
                    UserGroupUsers = new List<UserGroupUser>() { userGroupUser },
                    UserGroupPermissions = new List<UserGroupPermission>{ userGroupPermission }
                };

                before = DateTime.UtcNow;
                var createdUserGroup = await userGroupRepository.Create(userGroup);
                after = DateTime.UtcNow;

                Assert.Equal("New Group", createdUserGroup.Name);
                Assert.Equal("UGR_GUID", createdUserGroup.Id);
                Assert.Equal(createdUserGroup.CreatedAt, createdUserGroup.LastUpdatedAt);
                Assert.Equal(new[] { "UGR_GUID" }, createdUserGroup.UserGroupPermissions.Select(s => s.UserGroupId).OrderBy(id => id));
                Assert.Equal(new[] { "UGR_GUID" }, createdUserGroup.UserGroupUsers.Select(s => s.UserGroupId).OrderBy(id => id));
                Assert.True(before < createdUserGroup.CreatedAt && createdUserGroup.CreatedAt < after);
            }

            using (var context = new ARCDbContext(_options))
            {
                var userGroup = await context.UserGroups.FindAsync("UGR_GUID");

                Assert.Equal("New Group", userGroup.Name);
                Assert.Equal("UGR_GUID", userGroup.Id);               
                Assert.Equal(userGroup.CreatedAt, userGroup.LastUpdatedAt);
                Assert.True(before < userGroup.CreatedAt && userGroup.CreatedAt < after);
            }
        }

        [Fact]
        public void Create_userGroup_NullOrEmpty()
        {
            using (var context = new ARCDbContext(_options))
            {
                var mqIdGenerator = new Mock<IIdGenerator>();
                mqIdGenerator.Setup(ig => ig.GenerateId("UGR", It.IsAny<string>()))
                    .Returns("UGR_GUID");

                var userGroupRepository = new UserGroupRepositoryBuilder()
                    .WithARCDbContext(context)
                    .WithIdGenerator(mqIdGenerator.Object)
                    .Build();

                var userGroup = new UserGroup();

                var userGroupNullEmpty = Assert.ThrowsAsync<Exception>(async () => await userGroupRepository.Create(userGroup));

                Assert.Contains("The group name cannot be null", userGroupNullEmpty.Exception.Message);
            }
        }

        [Fact]
        public async Task Update_NewUserGroup_Success()
        {
            using (var context = new ARCDbContext(_options))
            {
                var userGroup = new UserGroup
                {
                    Id = "UGR_GUID",
                    Name = "New Group"
                };

                await context.UserGroups.AddAsync(userGroup);
                await context.SaveChangesAsync();
            }
            using (var context = new ARCDbContext(_options))
            {
                var userGroupRepository = new UserGroupRepositoryBuilder()
                    .WithARCDbContext(context)
                    .WithUpdateConcurrencyResolver(new UpdateConcurrencyResolver(context))
                    .Build();

                var userGroup = new UserGroup
                {
                    Id = "UGR_GUID",
                    Name = "Update Group"
                };

                var updatedUserGroup = await userGroupRepository.Update(userGroup);

                Assert.Equal("Update Group", updatedUserGroup.Name);
                Assert.Equal("UGR_GUID", updatedUserGroup.Id);
            }
        }

        [Fact]
        public async Task Remove_ExistentUser_Success()
        {
            using (var context = new ARCDbContext(_options))
            {
                var userGroup = new UserGroup
                {
                    Id = "UGR_GUID",
                    Name = "New Group"
                };

                await context.UserGroups.AddAsync(userGroup);
                await context.SaveChangesAsync();
            }

            using (var context = new ARCDbContext(_options))
            {
                var userGroupRepository = new UserGroupRepositoryBuilder()
                    .WithARCDbContext(context)
                    .Build();

                await userGroupRepository.Remove("UGR_GUID");
            }

            using (var context = new ARCDbContext(_options))
            {
                var removedUserGroup = await context.UserGroups.FindAsync("UGR_GUID");

                Assert.Null(removedUserGroup);
            }
        }

        [Fact]
        public void GetUserGroupById_Exception()
        {
            using (var context = new ARCDbContext(_options))
            {
                var userGroupRepository = new UserGroupRepositoryBuilder()
                    .WithARCDbContext(context)
                    .Build();

                var userGroup = new UserGroup
                {
                    Id = "UGR_GUID"
                };

                var userGroupByIdException = Assert.ThrowsAsync<Exception>(async () => await userGroupRepository.GetById(userGroup.Id));

                Assert.Contains("There's no user group with the ID 'UGR_GUID' on the database.", userGroupByIdException.Exception.Message);
            }
        }

        [Fact]
        public async Task GetUserGroupById_NoException()
        {
            using (var context = new ARCDbContext(_options))
            {
                var userGroup = new UserGroup
                {
                    Id = "UGR_GUID1"
                };

                await context.UserGroups.AddAsync(userGroup);
                await context.SaveChangesAsync();
            }

            using (var context = new ARCDbContext(_options))
            {
                var userGroupRepository = new UserGroupRepositoryBuilder()
                    .WithARCDbContext(context)
                    .Build();

                var userGroup = new UserGroup
                {
                    Id = "UGR_GUID1"
                };

                var userGroupByIdException = await userGroupRepository.GetById(userGroup.Id);

                Assert.NotNull(userGroupByIdException);
                Assert.Equal("UGR_GUID1", userGroupByIdException.Id);
            }
        }

        [Fact]
        public void GetUserGroupByName_Exception()
        {
            using (var context = new ARCDbContext(_options))
            {
                var userGroupRepository = new UserGroupRepositoryBuilder()
                    .WithARCDbContext(context)
                    .Build();

                var userGroup = new UserGroup
                {
                    Name = "New Group"
                };

                var userGroupByIdException = Assert.ThrowsAsync<Exception>(async () => await userGroupRepository.GetByName(userGroup.Name));

                Assert.Contains("There's no user group with the name 'New Group' on the database.", userGroupByIdException.Exception.Message);
            }
        }

        [Fact]
        public async Task GetUserGroupByName_NoException()
        {
            using (var context = new ARCDbContext(_options))
            {
                var userGroup = new UserGroup
                {
                    Id = "UGR_GUID",
                    Name = "New Group"
                };

                await context.UserGroups.AddAsync(userGroup);
                await context.SaveChangesAsync();
            }

            using (var context = new ARCDbContext(_options))
            {
                var userGroupRepository = new UserGroupRepositoryBuilder()
                    .WithARCDbContext(context)
                    .Build();

                var userGroup = new UserGroup
                {
                    Name = "New Group"
                };

                var userGroupByIdException = await userGroupRepository.GetByName(userGroup.Name);

                Assert.NotNull(userGroupByIdException);
                Assert.Equal("New Group", userGroupByIdException.Name);
            }
        }

        [Fact]
        public async Task GetAllUserGroup()
        {
            using (var context = new ARCDbContext(_options))
            {
                var userGroup1 = new UserGroup
                {
                    Id = "UGR_GUID1",
                    Name = "New Group1"
                };

                await context.UserGroups.AddAsync(userGroup1);
                await context.SaveChangesAsync();

                var userGroup2 = new UserGroup
                {
                    Id = "UGR_GUID2",
                    Name = "New Group2"
                };

                await context.UserGroups.AddAsync(userGroup2);
                await context.SaveChangesAsync();
            }

            using (var context = new ARCDbContext(_options))
            {
                var userGroupRepository = new UserGroupRepositoryBuilder()
                    .WithARCDbContext(context)
                    .Build();

                var allUserGroups = await userGroupRepository.GetAll();

                Assert.NotNull(allUserGroups);
                Assert.Equal(2, allUserGroups.Count);
            }
        }

        [Fact]
        public async Task GetUserGroups()
        {
            var userGroupIds = new string[]{ "UGR_GUID1" , "UGR_GUID2" };
            using (var context = new ARCDbContext(_options))
            {
                var userGroup1 = new UserGroup
                {
                    Id = "UGR_GUID1",
                    Name = "New Group1"
                };

                await context.UserGroups.AddAsync(userGroup1);
                await context.SaveChangesAsync();

                var userGroup2 = new UserGroup
                {
                    Id = "UGR_GUID2",
                    Name = "New Group2"
                };

                await context.UserGroups.AddAsync(userGroup2);
                await context.SaveChangesAsync();
            }

            using (var context = new ARCDbContext(_options))
            {
                var userGroupRepository = new UserGroupRepositoryBuilder()
                    .WithARCDbContext(context)
                    .Build();

                var allUserGroups = await userGroupRepository.GetUserGroups(userGroupIds);

                Assert.NotNull(allUserGroups);
                Assert.Equal(2, allUserGroups.Count());
            }
        }

        [Theory]
        [InlineData([null])]
        [InlineData([new string[] { }])]
        public async Task GetUserGroupsNullOrEmptyUserGroupIds(string[] userGroupIds)
        {
            using (var context = new ARCDbContext(_options))
            {
                var userGroup1 = new UserGroup
                {
                    Id = "UGR_GUID1",
                    Name = "New Group1"
                };

                await context.UserGroups.AddAsync(userGroup1);
                await context.SaveChangesAsync();

                var userGroup2 = new UserGroup
                {
                    Id = "UGR_GUID2",
                    Name = "New Group2"
                };

                await context.UserGroups.AddAsync(userGroup2);
                await context.SaveChangesAsync();
            }

            using (var context = new ARCDbContext(_options))
            {
                var userGroupRepository = new UserGroupRepositoryBuilder()
                    .WithARCDbContext(context)
                    .Build();

                var allUserGroups = await userGroupRepository.GetUserGroups(userGroupIds);

                Assert.Empty(allUserGroups);
            }
        }

        [Fact]
        public async Task GetUsers()
        {
            var userIds = new string[] { "USR_GUID1", "USR_GUID2" };
            using (var context = new ARCDbContext(_options))
            {
                var user1 = new User
                {
                    Id = "USR_GUID1",
                    Name = "New Group1"
                };

                await context.Users.AddAsync(user1);
                await context.SaveChangesAsync();

                var user2 = new User
                {
                    Id = "USR_GUID2",
                    Name = "New Group2"
                };

                await context.Users.AddAsync(user2);
                await context.SaveChangesAsync();
            }

            using (var context = new ARCDbContext(_options))
            {
                var userGroupRepository = new UserGroupRepositoryBuilder()
                    .WithARCDbContext(context)
                    .Build();

                var allUserGroups = await userGroupRepository.GetUsers(userIds);

                Assert.NotNull(allUserGroups);
                Assert.Equal(2, allUserGroups.Count());
            }
        }

        [Theory]
        [InlineData([null])]
        [InlineData([new string[] { }])]
        public async Task GetUsersNullOrEmptyUsersIds(string[] userIds)
        {
            using (var context = new ARCDbContext(_options))
            {
                var user1 = new User
                {
                    Id = "USR_GUID1",
                    Name = "New Group1"
                };

                await context.Users.AddAsync(user1);
                await context.SaveChangesAsync();

                var user2 = new User
                {
                    Id = "USR_GUID2",
                    Name = "New Group2"
                };

                await context.Users.AddAsync(user2);
                await context.SaveChangesAsync();
            }

            using (var context = new ARCDbContext(_options))
            {
                var userGroupRepository = new UserGroupRepositoryBuilder()
                    .WithARCDbContext(context)
                    .Build();

                var allUserGroups = await userGroupRepository.GetUsers(userIds);

                Assert.Empty(allUserGroups);
            }
        }
    }
}
