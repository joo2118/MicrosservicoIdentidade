using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Identidade.Dominio.Helpers;
using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Modelos;
using Identidade.Infraestrutura.ClientServices;
using Identidade.Publico.Dtos;
using Identidade.RESTAPI.Controllers;
using Identidade.UnitTests.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;


namespace Identidade.UnitTests.RESTAPI.Controllers
{
    public class UserGroupsControllerTests
    {
        public static IEnumerable<object?[]> GetUserGroupsControllersConstructorTestParameters()
        {
            return ParameterTestHelper.GetParameters(s => s
                .AddNullableParameter("userGroupService", Substitute.For<IUserGroupClientService>())
                .AddNullableParameter("credentialsFactory", Substitute.For<ICredentialsFactory>()));
        }

        [Theory]
        [MemberData(nameof(GetUserGroupsControllersConstructorTestParameters))]
        public void UserGroupsControllersConstructorTest(IUserGroupClientService userGroupService, ICredentialsFactory credentialsFactory,
            string? missingParameterName = null)
        {
            UserGroupsController Create() => new UserGroupsController(userGroupService, credentialsFactory);

            if (string.IsNullOrWhiteSpace(missingParameterName))
            {
                var userGroupsController = Create();

                Assert.NotNull(userGroupsController);
            }
            else
            {
                var ex = Assert.Throws<ArgumentNullException>(Create);
                Assert.Equal(missingParameterName, ex.ParamName);
            }
        }

        [Fact]
        public async Task Delete_NoContentSuccess_Test()
        {
            var userGroupID = "ExistentUserGroupID";
            var authorization = "Bearer TestAuthorization";

            var credentials = new Credentials("TestMultitenantId", "TestUserLogin");

            var credentialsFactoryMock = Substitute.For<ICredentialsFactory>();
            credentialsFactoryMock.Create(authorization).Returns(credentials);

            var userServiceMock = Substitute.For<IUserGroupClientService>();

            var userGroupsController = new UserGroupsController(userServiceMock, credentialsFactoryMock);
            var actionResult = await userGroupsController.Delete(userGroupID, authorization);

            var notFoundResult = Assert.IsType<NoContentResult>(actionResult);
            Assert.Equal(StatusCodes.Status204NoContent, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task Delete_RequestUser_NoContentSuccess_Test()
        {
            var requestUser = "TestRequestUser";
            var userGroupID = "ExistentUserGroupID";
            var authorization = "Bearer TestAuthorization";

            var credentials = new Credentials("TestMultitenantId", "TestUserLogin");

            var credentialsFactoryMock = Substitute.For<ICredentialsFactory>();
            credentialsFactoryMock.Create(authorization, requestUser).Returns(credentials);

            var userServiceMock = Substitute.For<IUserGroupClientService>();

            var userGroupsController = new UserGroupsController(userServiceMock, credentialsFactoryMock);
            var actionResult = await userGroupsController.Delete(userGroupID, authorization, requestUser);

            var notFoundResult = Assert.IsType<NoContentResult>(actionResult);
            Assert.Equal(StatusCodes.Status204NoContent, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task Delete_UserNotFound_Test()
        {
            var userGroupID = "NonExistentUserGroupID";
            var authorization = "Bearer TestAuthorization";

            var credentials = new Credentials("TestMultitenantId", "TestUserLogin");

            var credentialsFactoryMock = Substitute.For<ICredentialsFactory>();
            credentialsFactoryMock.Create(authorization).Returns(credentials);

            var userServiceMock = Substitute.For<IUserGroupClientService>();
            userServiceMock.When(x => x.Delete(userGroupID, credentials.UserLogin))
                           .Do(x => throw new NotFoundAppException("User Group not found."));

            var userGroupsController = new UserGroupsController(userServiceMock, credentialsFactoryMock);
            var actionResult = await userGroupsController.Delete(userGroupID, authorization);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task GetById_UserExists_Test()
        {
            var userGroupID = "ExistingUserGroupID";
            var expectedUserDto = new OutputUserGroupDto { Name = "Existing User Group Name" };

            var userServiceMock = Substitute.For<IUserGroupClientService>();
            userServiceMock.GetById(userGroupID).Returns(expectedUserDto);

            var userGroupsController = new UserGroupsController(userServiceMock, Substitute.For<ICredentialsFactory>());
            var actionResult = await userGroupsController.GetById(userGroupID);

            var okObjectResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);
            Assert.Equal(expectedUserDto, okObjectResult.Value);
        }

        [Fact]
        public async Task GetById_UserNotFound_Test()
        {
            var userGroupID = "NonExistentUserGroupID";

            var userServiceMock = Substitute.For<IUserGroupClientService>();
            userServiceMock.GetById(userGroupID).Throws(new NotFoundAppException("User Group not found."));

            var userGroupsController = new UserGroupsController(userServiceMock, Substitute.For<ICredentialsFactory>());
            var actionResult = await userGroupsController.GetById(userGroupID);

            Assert.IsType<NotFoundObjectResult>(actionResult);
        }

        [Fact]
        public async Task Get_UsersExist_Test()
        {
            var loginQuery = "UserLogin";
            var expectedUsersDto = new List<OutputUserGroupDto>
            {
                new OutputUserGroupDto { Name = "User Group Name 1" },
                new OutputUserGroupDto { Name = "User Group Name 2" }
            };

            var userServiceMock = Substitute.For<IUserGroupClientService>();
            userServiceMock.Get(loginQuery).Returns(expectedUsersDto);

            var userGroupsController = new UserGroupsController(userServiceMock, Substitute.For<ICredentialsFactory>());
            var actionResult = await userGroupsController.Get(loginQuery);

            var okObjectResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);
            Assert.Equal(expectedUsersDto, okObjectResult.Value);
        }

        [Fact]
        public async Task Get_NotFound_Test()
        {
            var loginQuery = "UserLogin";

            var userServiceMock = Substitute.For<IUserGroupClientService>();
            userServiceMock.Get(loginQuery).Throws(new NotFoundAppException("Test not found exception"));

            var userGroupsController = new UserGroupsController(userServiceMock, Substitute.For<ICredentialsFactory>());
            var actionResult = await userGroupsController.Get(loginQuery);

            var okObjectResult = Assert.IsType<NotFoundObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status404NotFound, okObjectResult.StatusCode);
        }

        [Fact]
        public async Task Update_Success_Test()
        {
            var userGroupId = "ExistingUserGroupId";
            var inputUserGroupDto = new InputUserGroupDto()
            {
                Name = "Updated User Group Name"
            };
            var authorization = "Bearer TestAuthorization";
            var updatedUserGroupDto = new OutputUserGroupDto { Name = "Updated User Group Name" };

            var credentials = new Credentials("TestMultitenantId", "TestUserLogin");

            var credentialsFactoryMock = Substitute.For<ICredentialsFactory>();
            credentialsFactoryMock.Create(authorization).Returns(credentials);

            var userServiceMock = Substitute.For<IUserGroupClientService>();
            userServiceMock.Update(userGroupId, inputUserGroupDto, credentials.UserLogin).Returns(updatedUserGroupDto);

            var userGroupsController = new UserGroupsController(userServiceMock, credentialsFactoryMock);
            var actionResult = await userGroupsController.Update(userGroupId, inputUserGroupDto, authorization);

            var okObjectResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);
            Assert.Equal(updatedUserGroupDto, okObjectResult.Value);
        }

        [Fact]
        public async Task Update_Success_RequestUser_Test()
        {
            var requestUser = "TestRequestUser";
            var userGroupId = "ExistingUserGroupId";
            var inputUserGroupDto = new InputUserGroupDto()
            {
                Name = "Updated User Group Name"
            };
            var authorization = "Bearer TestAuthorization";
            var updatedUserGroupDto = new OutputUserGroupDto { Name = "Updated User Group Name" };

            var credentials = new Credentials("TestMultitenantId", "TestUserLogin");

            var credentialsFactoryMock = Substitute.For<ICredentialsFactory>();
            credentialsFactoryMock.Create(authorization, requestUser).Returns(credentials);

            var userServiceMock = Substitute.For<IUserGroupClientService>();
            userServiceMock.Update(userGroupId, inputUserGroupDto, credentials.UserLogin).Returns(updatedUserGroupDto);

            var userGroupsController = new UserGroupsController(userServiceMock, credentialsFactoryMock);
            var actionResult = await userGroupsController.Update(userGroupId, inputUserGroupDto, authorization, requestUser);

            var okObjectResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);
            Assert.Equal(updatedUserGroupDto, okObjectResult.Value);
        }

        [Fact]
        public async Task Update_BadRequest_Test()
        {
            var userGroupId = "NonExistentUserGroupId";

            var authorization = "Bearer TestAuthorization";
            var credentials = new Credentials("TestMultitenantId", "TestUserLogin");

            var credentialsFactoryMock = Substitute.For<ICredentialsFactory>();
            credentialsFactoryMock.Create(authorization).Returns(credentials);

            var userServiceMock = Substitute.For<IUserGroupClientService>();
            userServiceMock.Update(userGroupId, Arg.Any<InputUserGroupDto>(), Arg.Any<string>()).Throws(new AppException("User Group Null"));

            var userGroupsController = new UserGroupsController(userServiceMock, credentialsFactoryMock);
            var actionResult = await userGroupsController.Update(userGroupId, (InputUserGroupDto)null, authorization);

            var notFoundResult = Assert.IsType<BadRequestObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status400BadRequest, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task Update_NotFound_Test()
        {
            var userGroupId = "NonExistentUserGroupId";
            var inputUserGroupDto = new InputUserGroupDto()
            {
                Name = "Non Existent User Group Name"
            };
            var authorization = "Bearer TestAuthorization";
            var credentials = new Credentials("TestMultitenantId", "TestUserLogin");

            var credentialsFactoryMock = Substitute.For<ICredentialsFactory>();
            credentialsFactoryMock.Create(authorization).Returns(credentials);

            var userServiceMock = Substitute.For<IUserGroupClientService>();
            userServiceMock.When(x => x.Update(userGroupId, inputUserGroupDto, credentials.UserLogin))
                           .Do(x => throw new NotFoundAppException("User Group not found."));

            var userGroupsController = new UserGroupsController(userServiceMock, credentialsFactoryMock);
            var actionResult = await userGroupsController.Update(userGroupId, inputUserGroupDto, authorization);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task Create_Success_Test()
        {
            var inputUserGroupDto = new InputUserGroupDto()
            {
                Name = "New User Group"
            };

            var authorization = "Bearer valid_token";
            var createdUserDto = new OutputUserGroupDto { Name = "New User Group" };

            var _userService = Substitute.For<IUserGroupClientService>();
            var _credentialsFactory = Substitute.For<ICredentialsFactory>();
            var _controller = new UserGroupsController(_userService, _credentialsFactory);

            _userService.Create(inputUserGroupDto, Arg.Any<string>()).Returns(createdUserDto);
            _credentialsFactory.Create(authorization).Returns(new Credentials("tenantId", "userLogin"));

            var result = await _controller.Create(inputUserGroupDto, authorization);

            var createdAtResult = Assert.IsType<CreatedResult>(result);
            Assert.Equal(StatusCodes.Status201Created, createdAtResult.StatusCode);
            Assert.Equal(createdUserDto, createdAtResult.Value);
        }

        [Fact]
        public async Task Create_Success_RequestUser_Test()
        {
            var requestUser = "TestRequestUser";
            var inputUserGroupDto = new InputUserGroupDto()
            {
                Name = "New User Group"
            };

            var authorization = "Bearer valid_token";
            var createdUserDto = new OutputUserGroupDto { Name = "New User Group" };

            var _userService = Substitute.For<IUserGroupClientService>();
            var _credentialsFactory = Substitute.For<ICredentialsFactory>();
            var _controller = new UserGroupsController(_userService, _credentialsFactory);

            _userService.Create(inputUserGroupDto, Arg.Any<string>()).Returns(createdUserDto);
            _credentialsFactory.Create(authorization, requestUser).Returns(new Credentials("tenantId", "userLogin"));

            var result = await _controller.Create(inputUserGroupDto, authorization, requestUser);

            var createdAtResult = Assert.IsType<CreatedResult>(result);
            Assert.Equal(StatusCodes.Status201Created, createdAtResult.StatusCode);
            Assert.Equal(createdUserDto, createdAtResult.Value);
        }

        [Fact]
        public async Task Create_BadRequest_NullUser_Test()
        {
            var authorization = "Bearer valid_token";

            var _userService = Substitute.For<IUserGroupClientService>();
            var _credentialsFactory = Substitute.For<ICredentialsFactory>();
            var _controller = new UserGroupsController(_userService, _credentialsFactory);

            _userService.Create(Arg.Any<InputUserGroupDto>(), Arg.Any<string>()).Throws(new AppException("User Group Null"));
            _credentialsFactory.Create(authorization).Returns(new Credentials("tenantId", "userLogin"));

            var result = await _controller.Create((InputUserGroupDto)null, authorization);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task Create_NotFound_Test()
        {
            var inputUserGroupDto = new InputUserGroupDto()
            {
                Name = "Existent User Group"
            };

            var authorization = "Bearer valid_token";

            var _userService = Substitute.For<IUserGroupClientService>();
            var _credentialsFactory = Substitute.For<ICredentialsFactory>();
            var _controller = new UserGroupsController(_userService, _credentialsFactory);

            _userService.Create(inputUserGroupDto, Arg.Any<string>()).Throws(new NotFoundAppException("User Group not found."));
            _credentialsFactory.Create(authorization).Returns(new Credentials("tenantId", "userLogin"));

            var result = await _controller.Create(inputUserGroupDto, authorization);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task Create_Conflict_Test()
        {
            var inputUserGroupDto = new InputUserGroupDto()
            {
                Name = "Existent User Group"
            };

            var authorization = "Bearer valid_token";

            var _userService = Substitute.For<IUserGroupClientService>();
            var _credentialsFactory = Substitute.For<ICredentialsFactory>();
            var _controller = new UserGroupsController(_userService, _credentialsFactory);

            _userService.Create(inputUserGroupDto, Arg.Any<string>()).Throws(new ConflictAppException("Conflict test."));
            _credentialsFactory.Create(authorization).Returns(new Credentials("tenantId", "userLogin"));

            var result = await _controller.Create(inputUserGroupDto, authorization);

            var notFoundResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(StatusCodes.Status409Conflict, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task Create_BadRequest_Exception_Test()
        {
            var inputUserGroupDto = new InputUserGroupDto()
            {
                Name = "Existent User Group"
            };

            var authorization = "Bearer valid_token";

            var _userService = Substitute.For<IUserGroupClientService>();
            var _credentialsFactory = Substitute.For<ICredentialsFactory>();
            var _controller = new UserGroupsController(_userService, _credentialsFactory);

            _userService.Create(inputUserGroupDto, Arg.Any<string>()).Throws(new AppException("App Exception test."));
            _credentialsFactory.Create(authorization).Returns(new Credentials("tenantId", "userLogin"));

            var result = await _controller.Create(inputUserGroupDto, authorization);

            var notFoundResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task GetPermissions_Test()
        {
            var userGroupId = "ExistentUserGroupId";

            var _userService = Substitute.For<IUserGroupClientService>();
            var _credentialsFactory = Substitute.For<ICredentialsFactory>();
            var _controller = new UserGroupsController(_userService, _credentialsFactory);

            _userService.GetPermissions(userGroupId).Returns(Array.Empty<OutputPermissionDto>());

            var actionResult = await _controller.GetPermissions(userGroupId);

            var okObjectResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);
            Assert.Equal(Array.Empty<OutputPermissionDto>(), okObjectResult.Value);
        }

        [Fact]
        public async Task GetPermissions_NotFound_Test()
        {
            var userGroupId = "ExistentUserGroupId";

            var _userService = Substitute.For<IUserGroupClientService>();
            var _credentialsFactory = Substitute.For<ICredentialsFactory>();
            var _controller = new UserGroupsController(_userService, _credentialsFactory);

            _userService.GetPermissions(userGroupId).Throws(new NotFoundAppException("User Group not found."));

            var actionResult = await _controller.GetPermissions(userGroupId);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task AddPermissions_Test()
        {
            var userGroupId = "ExistentUserGroupId";
            var authorization = "Bearer valid_token";

            var _userService = Substitute.For<IUserGroupClientService>();
            var _credentialsFactory = Substitute.For<ICredentialsFactory>();

            _credentialsFactory.Create(authorization).Returns(new Credentials("tenantId", "userLogin"));
            var _controller = new UserGroupsController(_userService, _credentialsFactory);

            _userService.AddPermissions(userGroupId, Arg.Any<IReadOnlyCollection<InputPermissionDto>>(), Arg.Any<string>()).Returns(new OutputUserGroupDto());

            var actionResult = await _controller.AddPermissions(userGroupId, Array.Empty<InputPermissionDto>(), authorization);

            var okObjectResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);
        }

        [Fact]
        public async Task AddPermissions_NotFound_Test()
        {
            var userGroupId = "ExistentUserGroupId";
            var authorization = "Bearer valid_token";

            var _userService = Substitute.For<IUserGroupClientService>();
            var _credentialsFactory = Substitute.For<ICredentialsFactory>();

            _credentialsFactory.Create(authorization).Returns(new Credentials("tenantId", "userLogin"));

            var _controller = new UserGroupsController(_userService, _credentialsFactory);

            _userService.AddPermissions(userGroupId, Arg.Any<IReadOnlyCollection<InputPermissionDto>>(), Arg.Any<string>()).Throws(new NotFoundAppException("User Group not found."));

            var actionResult = await _controller.AddPermissions(userGroupId, Array.Empty<InputPermissionDto>(), authorization);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task DeletePermissions_Test()
        {
            var userGroupId = "ExistentUserGroupId";
            var authorization = "Bearer valid_token";

            var _userService = Substitute.For<IUserGroupClientService>();
            var _credentialsFactory = Substitute.For<ICredentialsFactory>();

            _credentialsFactory.Create(authorization).Returns(new Credentials("tenantId", "userLogin"));
            var _controller = new UserGroupsController(_userService, _credentialsFactory);

            _userService.DeletePermissions(userGroupId, Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<string>()).Returns(new OutputUserGroupDto());

            var actionResult = await _controller.DeletePermissions(userGroupId, Array.Empty<string>(), authorization);

            var okObjectResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);
        }

        [Fact]
        public async Task DeletePermissions_NotFound_Test()
        {
            var userGroupId = "ExistentUserGroupId";
            var authorization = "Bearer valid_token";

            var _userService = Substitute.For<IUserGroupClientService>();
            var _credentialsFactory = Substitute.For<ICredentialsFactory>();

            _credentialsFactory.Create(authorization).Returns(new Credentials("tenantId", "userLogin"));

            var _controller = new UserGroupsController(_userService, _credentialsFactory);

            _userService.DeletePermissions(userGroupId, Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<string>()).Throws(new NotFoundAppException("User Group not found."));

            var actionResult = await _controller.DeletePermissions(userGroupId, Array.Empty<string>(), authorization);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }
    }
}
