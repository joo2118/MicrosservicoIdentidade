using Moq;
using Identidade.Dominio.Helpers;
using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Modelos;
using Identidade.Dominio.Servicos;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Identidade.UnitTests
{
    public class UserValidatorTests
    {
        private readonly Mock<IUserRepository> _mqUserRepository;
        private readonly UserValidator _userValidator;

        public UserValidatorTests()
        {
            _mqUserRepository = new Mock<IUserRepository>();
            _userValidator = new UserValidator(_mqUserRepository.Object, Mock.Of<IUserGroupRepository>());

            _mqUserRepository.Setup(userRepository => userRepository.GetByName(It.IsAny<string>()))
                .Throws<NotFoundAppException>();
        }

        [Fact]
        public void Validade_NullUser_ThrowsAppException()
        {
            User user = new User { UserName = null };

            var exception = Assert.Throws<AppException>(() => _userValidator.Validate(user.UserName));

            Assert.Single(exception.Errors);
            Assert.Equal("The username can not be null.", exception.Errors.Single());
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        public void Validade_NullOrWhiteSpaceUserName_ThrowsAppException(string username)
        {
            User user = new User { UserName = username };

            var exception = Assert.Throws<AppException>(() => _userValidator.Validate(user.UserName));
            
            Assert.Single(exception.Errors);
            Assert.Equal("The username can not be empty.", exception.Errors.Single());
        }

        [Fact]
        public void Validade_ExistingUser_ThrowsConflictAppException()
        {
            _mqUserRepository.Setup(userManager => userManager.GetByName("username"))
                .Returns(Task.FromResult(new User()));

            User user = new User { UserName = "username" };

            var exception = Assert.Throws<ConflictAppException>(() => _userValidator.Validate(user.UserName));
            
            Assert.Single(exception.Errors);
            Assert.Equal("There already exists a user with the same username.", exception.Errors.Single());
        }

        [Theory]
        [InlineData("username")]
        [InlineData("user.name")]
        [InlineData("user_name")]
        [InlineData("username@mail.com")]
        public void Validade_ValidUser_ValidatesSuccessfully(string username)
        {
            var validationResult = _userValidator.Validate(username);

            Assert.True(validationResult.IsValid);
        }

        [Fact]
        public void VerifyExistences_ValidUsersAndGroups_DoesNotThrow()
        {
            var userIds = new[] { "user1", "user2" };
            var userGroupIds = new[] { "group1", "group2" };
            var users = new User[] { new User { Id = "user1" }, new User { Id = "user2" } };
            var userGroups = new UserGroup[] { new UserGroup { Id = "group1" }, new UserGroup { Id = "group2" } };

            var userGroupRepositoryMock = new Mock<IUserGroupRepository>();
            userGroupRepositoryMock.Setup(repo => repo.GetUsers(It.IsAny<string[]>()))
                .Returns(Task.FromResult(users));
            userGroupRepositoryMock.Setup(repo => repo.GetUserGroups(It.IsAny<string[]>()))
                .Returns(Task.FromResult(userGroups));

            var userValidator = new UserValidator(_mqUserRepository.Object, userGroupRepositoryMock.Object);

            var exception = Record.Exception(() => userValidator.VerifyExistences(userIds, userGroupIds));

            Assert.Null(exception);
        }

        [Fact]
        public void VerifyExistences_MissingUsers_ThrowsNotFoundAppException()
        {
            var userIds = new[] { "user1", "user2" };
            var userGroupIds = new[] { "group1", "group2" };
            var users = new User[] { new User { Id = "user1" } };
            var userGroups = new UserGroup[] { new UserGroup { Id = "group1" }, new UserGroup { Id = "group2" } };
            var expectedMessage = "There's no Users with the ID 'user2' on the database.";

            var userGroupRepositoryMock = new Mock<IUserGroupRepository>();
            userGroupRepositoryMock.Setup(repo => repo.GetUsers(It.IsAny<string[]>()))
                .Returns(Task.FromResult(users));
            userGroupRepositoryMock.Setup(repo => repo.GetUserGroups(It.IsAny<string[]>()))
                .Returns(Task.FromResult(userGroups));

            var userValidator = new UserValidator(_mqUserRepository.Object, userGroupRepositoryMock.Object);

            var exception = Assert.Throws<NotFoundAppException>(() => userValidator.VerifyExistences(userIds, userGroupIds));

            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public void VerifyExistences_MissingUserGroups_ThrowsNotFoundAppException()
        {
            var userIds = new[] { "user1", "user2" };
            var userGroupIds = new[] { "group1", "group2" };
            var users = new User[] { new User { Id = "user1" }, new User { Id = "user2" } };
            var userGroups = new UserGroup[] { new UserGroup { Id = "group1" } };
            var expectedMessage = "There's no User Groups with the ID 'group2' on the database.";

            var userGroupRepositoryMock = new Mock<IUserGroupRepository>();
            userGroupRepositoryMock.Setup(repo => repo.GetUsers(It.IsAny<string[]>()))
                .Returns(Task.FromResult(users));
            userGroupRepositoryMock.Setup(repo => repo.GetUserGroups(It.IsAny<string[]>()))
                .Returns(Task.FromResult(userGroups));

            var userValidator = new UserValidator(_mqUserRepository.Object, userGroupRepositoryMock.Object);

            var exception = Assert.Throws<NotFoundAppException>(() => userValidator.VerifyExistences(userIds, userGroupIds));

            Assert.Equal(expectedMessage, exception.Message);
        }

    }
}
