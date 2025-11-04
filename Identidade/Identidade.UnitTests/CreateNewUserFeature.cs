using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Identidade.Dominio.Helpers;
using Identidade.Dominio.Modelos;
using Identidade.Dominio.Servicos;
using Identidade.Infraestrutura.Data;
using Identidade.UnitTests.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Identidade.UnitTests
{
    [FeatureFile("./CreateNewUserFeature.feature")]
    public class CreateNewUserFeature : Feature, IDisposable
    {
        private User _createdUser;
        private DateTime _before;
        private DateTime _after;
        private Mock<IIdGenerator> _mqIdGenerator = new Mock<IIdGenerator>();
        private Mock<IUserValidator> _mqUserNameValidator = new Mock<IUserValidator>();
        private Mock<IPasswordValidator> _mqPasswordValidator = new Mock<IPasswordValidator>();
        private ARCDbContext _context;
        private readonly DbContextOptions<ARCDbContext> _options;
        private readonly SqliteConnection _connection;
        private Exception _thrownException;

        public CreateNewUserFeature()
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

        [Given(@"the IdGenerator returning ""USR_GUID"" for GenerateId\(""USR""\)")]
        public void Given_the_IdGenerator_returning_URS_GUID_for_GenerateId_USR()
        {
            _mqIdGenerator = new Mock<IIdGenerator>();
            _mqIdGenerator.Setup(ig => ig.GenerateId("USR", It.IsAny<string>()))
                .Returns("USR_GUID");
        }

        [Given(@"the passwordValidator throwing AppException for Validate\(""password""\)")]
        public void Given_the_passwordValidator_throwing_AppException_for_Validate_password()
        {
            _mqPasswordValidator = new Mock<IPasswordValidator>();
            _mqPasswordValidator.Setup(ig => ig.Validate("password"))
                .Throws<AppException>();
        }

        [Given(@"the userValidator throwing AppException for Validate\(""new.user""\)")]
        public void Given_the_userValidator_throwing_AppException_for_Validate_new_user()
        {
            _mqUserNameValidator = new Mock<IUserValidator>();
            _mqUserNameValidator.Setup(ig => ig.Validate("new.user"))
                .Throws<AppException>();
        }

        [And(@"there exists user groups with IDs UGR_GUIDGroup1 and UGR_GUIDGroup2")]
        public async Task And_there_exists_user_groups_with_IDs_UGR_GUIDGroup1_and_UGR_GUIDGroup2()
        {
            using (var context = new ARCDbContext(_options))
            {
                var userGroup1 = new UserGroup
                {
                    Id = "USR_GUIDGROUP1",
                    Name = "Group 1"
                };

                var userGroup2 = new UserGroup
                {
                    Id = "USR_GUIDGROUP2",
                    Name = "Group 2"
                };

                await context.UserGroups.AddRangeAsync(userGroup1, userGroup2);
                await context.SaveChangesAsync();
            }
        }

        [And(@"there exists users with IDs USR_GUIDUser1 and USR_GUIDUser2")]
        public async Task And_there_exists_users_with_IDs_USR_GUIDUser1_and_USR_GUIDUser2()
        {
            using (var context = new ARCDbContext(_options))
            {
                var user1 = new User
                {
                    Id = "USR_GUIDUSER1",
                    Name = "User 1"
                };

                var user2 = new User
                {
                    Id = "USR_GUIDUSER2",
                    Name = "User 2"
                };

                await context.Users.AddRangeAsync(user1, user2);
                await context.SaveChangesAsync();
            }
        }

        [When(@"I input a new user and the password ""password""")]
        [When(@"I input a new user with username ""new.user""")]
        public async Task When_I_input_a_new_user_and_the_password_password()
        {
            _context = new ARCDbContext(_options);

            var userRepository = new UserRepositoryBuilder()
                .WithIdGenerator(_mqIdGenerator.Object)
                .WithUserNameValidator(_mqUserNameValidator.Object)
                .WithPasswordValidator(_mqPasswordValidator.Object)
                .WithARCDbContext(_context)
                .WithUpdateConcurrencyResolver(new UpdateConcurrencyResolver(_context))
                .Build();

            var user = new User
            {
                Name = "New User",
                UserName = "new.user",
                Email = "new.user@email.com"
            };

            try
            {
                _thrownException = null;
                _before = DateTime.UtcNow;
                _createdUser = await userRepository.Create(user, "password");
                _after = DateTime.UtcNow;
            }
            catch (Exception e)
            {
                _thrownException = e;
            }
        }

        [When(@"I input a new user with the groups 1 and 2")]
        public async Task When_I_input_a_new_user_with_the_groups_1_and_2()
        {
            _context = new ARCDbContext(_options);

            var userRepository = new UserRepositoryBuilder()
                .WithIdGenerator(_mqIdGenerator.Object)
                .WithARCDbContext(_context)
                .WithUpdateConcurrencyResolver(new UpdateConcurrencyResolver(_context))
                .Build();

            var user = new User
            {
                Name = "New User",
                UserName = "new.user"
            };


            foreach (var userGroup in _context.UserGroups)
                user.UserGroupUsers.Add(new UserGroupUser(userGroup, user));

            _createdUser = await userRepository.Create(user, "password");
        }

        [When(@"I input a new user with the substitutes 1 and 2")]
        public async Task When_I_input_a_new_user_with_the_substitutes_1_and_2()
        {
            _context = new ARCDbContext(_options);

            var userRepository = new UserRepositoryBuilder()
                .WithIdGenerator(_mqIdGenerator.Object)
                .WithARCDbContext(_context)
                .WithUpdateConcurrencyResolver(new UpdateConcurrencyResolver(_context))
                .Build();

            var user = new User
            {
                Name = "New User",
                UserName = "new.user"
            };


            foreach (var substitute in _context.Users.Where(u => u.Name != "Administrator"))
                user.UserSubstitutions.Add(new UserSubstitution(user, substitute));

            _createdUser = await userRepository.Create(user, "password");
        }

        [Then(@"the username and the password are validated")]
        public void Then_the_username_and_the_password_are_validated()
        {
            _mqUserNameValidator.Verify(uv => uv.Validate(_createdUser.UserName), Times.Once);
            _mqPasswordValidator.Verify(pv => pv.Validate("password"), Times.Once);
        }

        [And(@"the user is created successfully")]
        public async Task And_the_user_is_created_successfully()
        {
            Assert.Null(_thrownException);
            Assert.Equal("New User", _createdUser.Name);
            Assert.Equal("new.user", _createdUser.UserName);
            Assert.Equal("new.user@email.com", _createdUser.Email);
            Assert.Equal("USR_GUID", _createdUser.Id);
            Assert.Equal("EncriptedPassword", _createdUser.PasswordHash);
            Assert.Equal("EncriptedPassword", _createdUser.PasswordHistory);
            Assert.Equal(_createdUser.CreatedAt, _createdUser.LastUpdatedAt);
            Assert.True(_before < _createdUser.CreatedAt && _createdUser.CreatedAt < _after);

            _context.Dispose();

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
                Assert.True(_before < user.CreatedAt && user.CreatedAt < _after);
            }
        }

        [Then(@"the user is created with the groups 1 and 2")]
        public async Task And_the_user_is_created_with_the_user_groups_1_and_2()
        {
            Assert.Equal("New User", _createdUser.Name);
            Assert.Equal("USR_GUID", _createdUser.Id);
            Assert.Equal(new[] {"Group 1", "Group 2"}, _createdUser.UserGroupUsers.Select(ugu => ugu.UserGroup.Name).OrderBy(name => name));

            _context.Dispose();

            using (var context = new ARCDbContext(_options))
            {
                var user = await context.Users
                    .Include(u => u.UserGroupUsers)
                    .ThenInclude(ugu => ugu.UserGroup)
                    .SingleOrDefaultAsync(u => u.Id == "USR_GUID");

                Assert.Equal("New User", user.Name);
                Assert.Equal("USR_GUID", user.Id);
                Assert.Equal(new[] {"Group 1", "Group 2"}, user.UserGroupUsers.Select(ugu => ugu.UserGroup.Name).OrderBy(name => name));
            }
        }

        [Then(@"the user is created with the substitutes 1 and 2")]
        public async Task And_the_user_is_created_with_the_substitutes_1_and_2()
        {
            Assert.Equal("New User", _createdUser.Name);
            Assert.Equal("USR_GUID", _createdUser.Id);
            Assert.Equal(new[] {"User 1", "User 2"}, _createdUser.UserSubstitutions.Select(us => us.SubstituteUser.Name).OrderBy(name => name));

            _context.Dispose();

            using (var context = new ARCDbContext(_options))
            {
                var user = await context.Users
                    .Include(u => u.UserSubstitutions)
                    .ThenInclude(us => us.SubstituteUser)
                    .SingleOrDefaultAsync(u => u.Id == "USR_GUID");

                Assert.Equal("New User", user.Name);
                Assert.Equal("USR_GUID", user.Id);
                Assert.Equal(new[] {"User 1", "User 2"}, user.UserSubstitutions.Select(us => us.SubstituteUser.Name).OrderBy(name => name));
            }
        }

        [Then(@"an AppException is thrown")]
        public void Then_an_AppException_is_thrown()
        {
            Assert.NotNull(_thrownException);
            Assert.IsType<AppException>(_thrownException);
        }

        public void Dispose()
        {
            _connection.Close();
        }
    }
}
