using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Identidade.Dominio.Helpers;
using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Modelos;
using Identidade.Infraestrutura.ClientServices;
using Identidade.Infraestrutura.Helpers;
using Identidade.Publico.Dtos;
using Identidade.Publico.Enumerations;
using Identidade.UnitTests.Helpers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Identidade.RESTAPI.Controladores;

namespace Identidade.UnitTests.RESTAPI.Controllers
{
    public class UsersControllerTests
    {
        private static TelemetryClient CreateTelemetryClient()
        {
            var telemetryConfiguration = new TelemetryConfiguration
            {
                TelemetryChannel = new InMemoryChannel()
            };

            return new TelemetryClient(telemetryConfiguration);
        }

        public static IEnumerable<object?[]> GetUsersControllersConstructorTestParameters()
        {
            return ParameterTestHelper.GetParameters(s => s
                .AddNullableParameter("userService", Substitute.For<IUserClientService>())
                .AddNullableParameter("credentialsFactory", Substitute.For<ICredentialsFactory>())
                .AddNullableParameter("logger", Substitute.For<ILogger>())
                .AddNullableParameter("telemetryClient", CreateTelemetryClient()));
        }

        [Theory]
        [MemberData(nameof(GetUsersControllersConstructorTestParameters))]
        public void UsersControllersConstructorTest(IUserClientService userService, ICredentialsFactory credentialsFactory, ILogger logger,
            TelemetryClient telemetryClient,
            string? missingParameterName = null)
        {
            UsersController Create() => new UsersController(userService, credentialsFactory, logger, telemetryClient);

            if (string.IsNullOrWhiteSpace(missingParameterName))
            {
                var usersController = Create();

                Assert.NotNull(usersController);
            }
            else
            {
                var ex = Assert.Throws<ArgumentNullException>(Create);
                Assert.Equal(missingParameterName, ex.ParamName);
            }
        }

        [Fact]
        public void Patch_Test()
        {
            var userId = "TestUserId";
            var authorization = "TestAuthorization";
            var password = "TestPassword";

            var currentUser = new OutputUserDto()
            {
                Login = "CurrentLogin",
                Name = "CurrentName"
            };

            var patchUser = new PatchUserDto()
            {
                Login = "NewLogin",
                Name = "NewName"
            };

            var mergedUser = new ArcUserDto(
                email: "Merged@Email.com",
                password: "MergedPassword",
                passwordExpiration: new DateTime(2021, 2, 3).ToString(),
                passwordDoesNotExpire: true,
                active: true,
                authenticationType: AuthenticationType.DatabaseUser,
                language: Language.Portugues)
            {
                Login = "MergedLogin",
                Name = "MergedName"
            };

            var expected = new OutputUserDto()
            {
                Login = "ExpectedLogin",
                Name = "ExpectedName"
            };

            var credentials = new Credentials("TestMultitenantId", "TestUserLogin");

            var mqCredentialsFactory = Substitute.For<ICredentialsFactory>();

            mqCredentialsFactory
                .Create(authorization)
                .Returns(credentials);

            var mqUserService = Substitute.For<IUserClientService>();

            mqUserService
                .GetById(userId, out Arg.Any<string>())
                .Returns(x =>
                {
                    x[1] = password;
                    return currentUser;
                });

            mqUserService
                .Update(userId, mergedUser, credentials.UserLogin)
                .Returns(expected);

            var mqPatchUserMerger = Substitute.For<IPatchUserMerger>();

            mqPatchUserMerger
                .Merge(currentUser, password, patchUser)
                .Returns(mergedUser);

            var usersController = new UsersController(mqUserService, mqCredentialsFactory, Substitute.For<ILogger>(), CreateTelemetryClient());
            var actual = usersController.Patch(userId, patchUser, authorization, mqPatchUserMerger).Result as ObjectResult;

            Assert.Equal(StatusCodes.Status200OK, actual.StatusCode);
            Assert.Equal(expected, actual.Value);
        }

        [Fact]
        public void Patch_BadRequest_Test()
        {
            var userId = "TestUserId";
            var authorization = "TestAuthorization";

            var mqCredentialsFactory = Substitute.For<ICredentialsFactory>();

            var usersController = new UsersController(Substitute.For<IUserClientService>(), mqCredentialsFactory, Substitute.For<ILogger>(), CreateTelemetryClient());
            var actual = usersController.Patch(userId, (PatchUserDto)null, authorization, Substitute.For<IPatchUserMerger>()).Result as ObjectResult;

            Assert.Equal(StatusCodes.Status400BadRequest, actual.StatusCode);
        }

        [Fact]
        public void Patch_NotFound_Test()
        {
            var userId = "TestUserId";
            var authorization = "TestAuthorization";

            var patchUser = new PatchUserDto()
            {
                Login = "NewLogin",
                Name = "NewName"
            };

            var credentials = new Credentials("TestMultitenantId", "TestUserLogin");

            var mqCredentialsFactory = Substitute.For<ICredentialsFactory>();

            mqCredentialsFactory
                .Create(authorization)
                .Returns(credentials);

            var usersController = new UsersController(Substitute.For<IUserClientService>(), mqCredentialsFactory, Substitute.For<ILogger>(), CreateTelemetryClient());
            var actual = usersController.Patch(userId, patchUser, authorization, Substitute.For<IPatchUserMerger>()).Result as ObjectResult;

            Assert.Equal(StatusCodes.Status404NotFound, actual.StatusCode);
        }

        [Fact]
        public void Patch_Error_Test()
        {
            var userId = "TestUserId";
            var authorization = "TestAuthorization";

            var patchUser = new PatchUserDto()
            {
                Login = "NewLogin",
                Name = "NewName"
            };

            var credentials = new Credentials("TestMultitenantId", "TestUserLogin");

            var mqCredentialsFactory = Substitute.For<ICredentialsFactory>();

            mqCredentialsFactory
                .Create(authorization)
                .Returns(credentials);

            var mqUserService = Substitute.For<IUserClientService>();

            mqUserService
                .When(x => x.GetById(userId, out Arg.Any<string>()))
                .Do(x => throw new Exception("TestException"));

            var usersController = new UsersController(mqUserService, mqCredentialsFactory, Substitute.For<ILogger>(), CreateTelemetryClient());
            var actual = usersController.Patch(userId, patchUser, authorization, Substitute.For<IPatchUserMerger>()).Result as ObjectResult;

            Assert.Equal(StatusCodes.Status500InternalServerError, actual.StatusCode);
        }

        [Fact]
        public async Task Delete_NoContentSuccess_Test()
        {
            var userId = "ExistentUserId";
            var authorization = "Bearer TestAuthorization";

            var credentials = new Credentials("TestMultitenantId", "TestUserLogin");

            var credentialsFactoryMock = Substitute.For<ICredentialsFactory>();
            credentialsFactoryMock.Create(authorization).Returns(credentials);

            var userServiceMock = Substitute.For<IUserClientService>();

            var usersController = new UsersController(userServiceMock, credentialsFactoryMock, Substitute.For<ILogger>(), CreateTelemetryClient());
            var actionResult = await usersController.Delete(userId, authorization);

            var notFoundResult = Assert.IsType<NoContentResult>(actionResult);
            Assert.Equal(StatusCodes.Status204NoContent, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task Delete_RequestUser_NoContentSuccess_Test()
        {
            var requestUser = "TestRequestUser";
            var userId = "ExistentUserId";
            var authorization = "Bearer TestAuthorization";

            var credentials = new Credentials("TestMultitenantId", "TestUserLogin");

            var credentialsFactoryMock = Substitute.For<ICredentialsFactory>();
            credentialsFactoryMock.Create(authorization, requestUser).Returns(credentials);

            var userServiceMock = Substitute.For<IUserClientService>();

            var usersController = new UsersController(userServiceMock, credentialsFactoryMock, Substitute.For<ILogger>(), CreateTelemetryClient());
            var actionResult = await usersController.Delete(userId, authorization, requestUser);

            var notFoundResult = Assert.IsType<NoContentResult>(actionResult);
            Assert.Equal(StatusCodes.Status204NoContent, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task Delete_UserNotFound_Test()
        {
            var userId = "NonExistentUserId";
            var authorization = "Bearer TestAuthorization";

            var credentials = new Credentials("TestMultitenantId", "TestUserLogin");

            var credentialsFactoryMock = Substitute.For<ICredentialsFactory>();
            credentialsFactoryMock.Create(authorization).Returns(credentials);

            var userServiceMock = Substitute.For<IUserClientService>();
            userServiceMock.When(x => x.Delete(userId, credentials.UserLogin))
                           .Do(x => throw new NotFoundAppException("User not found."));

            var usersController = new UsersController(userServiceMock, credentialsFactoryMock, Substitute.For<ILogger>(), CreateTelemetryClient());
            var actionResult = await usersController.Delete(userId, authorization);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task GetById_UserExists_Test()
        {
            var userId = "ExistingUserId";
            var expectedUserDto = new OutputUserDto { Login = "ExistingUser", Name = "Existing User Name" };

            var userServiceMock = Substitute.For<IUserClientService>();
            userServiceMock.GetById(userId).Returns(expectedUserDto);

            var usersController = new UsersController(userServiceMock, Substitute.For<ICredentialsFactory>(), Substitute.For<ILogger>(), CreateTelemetryClient());
            var actionResult = await usersController.GetById(userId);

            var okObjectResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);
            Assert.Equal(expectedUserDto, okObjectResult.Value);
        }

        [Fact]
        public async Task GetById_UserNotFound_Test()
        {
            var userId = "NonExistentUserId";

            var userServiceMock = Substitute.For<IUserClientService>();
            userServiceMock.GetById(userId).Throws(new NotFoundAppException("User not found."));

            var usersController = new UsersController(userServiceMock, Substitute.For<ICredentialsFactory>(), Substitute.For<ILogger>(), CreateTelemetryClient());
            var actionResult = await usersController.GetById(userId);

            Assert.IsType<NotFoundObjectResult>(actionResult);
        }

        [Fact]
        public async Task Get_UsersExist_Test()
        {
            var loginQuery = "UserLogin";
            var expectedUsersDto = new List<OutputUserDto>
            {
                new OutputUserDto { Login = "User1", Name = "User Name 1" },
                new OutputUserDto { Login = "User2", Name = "User Name 2" }
            };

            var userServiceMock = Substitute.For<IUserClientService>();
            userServiceMock.Get(loginQuery).Returns(expectedUsersDto);

            var usersController = new UsersController(userServiceMock, Substitute.For<ICredentialsFactory>(), Substitute.For<ILogger>(), CreateTelemetryClient());
            var actionResult = await usersController.Get(loginQuery);

            var okObjectResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);
            Assert.Equal(expectedUsersDto, okObjectResult.Value);
        }

        [Fact]
        public async Task Get_NotFound_Test()
        {
            var loginQuery = "UserLogin";

            var userServiceMock = Substitute.For<IUserClientService>();
            userServiceMock.Get(loginQuery).Throws(new NotFoundAppException("Test not found exception"));

            var usersController = new UsersController(userServiceMock, Substitute.For<ICredentialsFactory>(), Substitute.For<ILogger>(), CreateTelemetryClient());
            var actionResult = await usersController.Get(loginQuery);

            var okObjectResult = Assert.IsType<NotFoundObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status404NotFound, okObjectResult.StatusCode);
        }

        [Fact]
        public async Task GetUserGroups_UserExists_Test()
        {
            var userId = "ExistingUserId";
            var expectedGroupsDto = new List<OutputUserGroupDto>
            {
                new OutputUserGroupDto { Id = "Group1", Name = "Group Name 1" },
                new OutputUserGroupDto { Id = "Group2", Name = "Group Name 2" }
            };

            var userServiceMock = Substitute.For<IUserClientService>();
            userServiceMock.GetUserGroups(userId).Returns(expectedGroupsDto);

            var usersController = new UsersController(userServiceMock, Substitute.For<ICredentialsFactory>(), Substitute.For<ILogger>(), CreateTelemetryClient());
            var actionResult = await usersController.GetUserGroups(userId);

            var okObjectResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);
            Assert.Equal(expectedGroupsDto, okObjectResult.Value);
        }

        [Fact]
        public async Task GetUserGroups_BadRequest_Test()
        {
            var userId = "NonExistingUserId";

            var userServiceMock = Substitute.For<IUserClientService>();
            userServiceMock.GetUserGroups(userId).Throws(new AppException("Test bad request"));

            var usersController = new UsersController(userServiceMock, Substitute.For<ICredentialsFactory>(), Substitute.For<ILogger>(), CreateTelemetryClient());
            var actionResult = await usersController.GetUserGroups(userId);

            var okObjectResult = Assert.IsType<BadRequestObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status400BadRequest, okObjectResult.StatusCode);
        }

        [Fact]
        public async Task Update_Success_Test()
        {
            var userId = "ExistingUserId";
            var arcUserDto = new ArcUserDto("email@test.com", "password", null, false, true, AuthenticationType.DatabaseUser, Language.Ingles)
            {
                Login = "UpdatedUser",
                Name = "Updated User Name"
            };
            var authorization = "Bearer TestAuthorization";
            var updatedUserDto = new OutputUserDto { Login = "UpdatedUser", Name = "Updated User Name" };

            var credentials = new Credentials("TestMultitenantId", "TestUserLogin");

            var credentialsFactoryMock = Substitute.For<ICredentialsFactory>();
            credentialsFactoryMock.Create(authorization).Returns(credentials);

            var userServiceMock = Substitute.For<IUserClientService>();
            userServiceMock.Update(userId, arcUserDto, credentials.UserLogin).Returns(updatedUserDto);

            var usersController = new UsersController(userServiceMock, credentialsFactoryMock, Substitute.For<ILogger>(), CreateTelemetryClient());
            var actionResult = await usersController.Update(userId, arcUserDto, authorization);

            var okObjectResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);
            Assert.Equal(updatedUserDto, okObjectResult.Value);
        }

        [Fact]
        public async Task Update_Success_RequestUser_Test()
        {
            var requestUser = "TestRequestUser";
            var userId = "ExistingUserId";
            var arcUserDto = new ArcUserDto("email@test.com", "password", null, false, true, AuthenticationType.DatabaseUser, Language.Ingles)
            {
                Login = "UpdatedUser",
                Name = "Updated User Name"
            };
            var authorization = "Bearer TestAuthorization";
            var updatedUserDto = new OutputUserDto { Login = "UpdatedUser", Name = "Updated User Name" };

            var credentials = new Credentials("TestMultitenantId", "TestUserLogin");

            var credentialsFactoryMock = Substitute.For<ICredentialsFactory>();
            credentialsFactoryMock.Create(authorization, requestUser).Returns(credentials);

            var userServiceMock = Substitute.For<IUserClientService>();
            userServiceMock.Update(userId, arcUserDto, credentials.UserLogin).Returns(updatedUserDto);

            var usersController = new UsersController(userServiceMock, credentialsFactoryMock, Substitute.For<ILogger>(), CreateTelemetryClient());
            var actionResult = await usersController.Update(userId, arcUserDto, authorization, requestUser);

            var okObjectResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);
            Assert.Equal(updatedUserDto, okObjectResult.Value);
        }

        [Fact]
        public async Task Update_BadRequest_Test()
        {
            var userId = "NonExistentUserId";
            var authorization = "Bearer TestAuthorization";

            var credentialsFactoryMock = Substitute.For<ICredentialsFactory>();

            var userServiceMock = Substitute.For<IUserClientService>();

            var usersController = new UsersController(userServiceMock, credentialsFactoryMock, Substitute.For<ILogger>(), CreateTelemetryClient());
            var actionResult = await usersController.Update(userId, (ArcUserDto)null, authorization);

            var notFoundResult = Assert.IsType<BadRequestObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status400BadRequest, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task Update_NotFound_Test()
        {
            var userId = "NonExistentUserId";
            var arcUserDto = new ArcUserDto("email@test.com", "password", null, false, true, AuthenticationType.DatabaseUser, Language.Ingles)
            {
                Login = "NonExistentUser",
                Name = "Non Existent User Name"
            };
            var authorization = "Bearer TestAuthorization";

            var credentials = new Credentials("TestMultitenantId", "TestUserLogin");

            var credentialsFactoryMock = Substitute.For<ICredentialsFactory>();
            credentialsFactoryMock.Create(authorization).Returns(credentials);

            var userServiceMock = Substitute.For<IUserClientService>();
            userServiceMock.When(x => x.Update(userId, arcUserDto, credentials.UserLogin))
                           .Do(x => throw new NotFoundAppException("User not found."));

            var usersController = new UsersController(userServiceMock, credentialsFactoryMock, Substitute.For<ILogger>(), CreateTelemetryClient());
            var actionResult = await usersController.Update(userId, arcUserDto, authorization);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task Update_InternalServerError_Test()
        {
            var userId = "ExistingUserId";
            var arcUserDto = new ArcUserDto("email@test.com", "password", null, false, true, AuthenticationType.DatabaseUser, Language.Ingles)
            {
                Login = "UpdatedUser",
                Name = "Updated User Name"
            };
            var authorization = "Bearer TestAuthorization";

            var credentials = new Credentials("TestMultitenantId", "TestUserLogin");

            var credentialsFactoryMock = Substitute.For<ICredentialsFactory>();
            credentialsFactoryMock.Create(authorization).Returns(credentials);

            var userServiceMock = Substitute.For<IUserClientService>();
            userServiceMock.When(x => x.Update(userId, arcUserDto, credentials.UserLogin))
                           .Do(x => throw new Exception("Internal server error"));

            var usersController = new UsersController(userServiceMock, credentialsFactoryMock, Substitute.For<ILogger>(), CreateTelemetryClient());
            var actionResult = await usersController.Update(userId, arcUserDto, authorization);

            var internalServerErrorResult = Assert.IsType<ObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status500InternalServerError, internalServerErrorResult.StatusCode);
        }

        [Fact]
        public async Task Create_Success_Test()
        {
            var arcUserDto = new ArcUserDto("email@test.com", "password", null, false, true, AuthenticationType.DatabaseUser, Language.Ingles)
            {
                Login = "newuser@example.com",
                Name = "New User"
            };

            var authorization = "Bearer valid_token";
            var createdUserDto = new OutputUserDto { Login = "newuser@example.com", Name = "New User" };

            var _userService = Substitute.For<IUserClientService>();
            var _credentialsFactory = Substitute.For<ICredentialsFactory>();
            var _controller = new UsersController(_userService, _credentialsFactory, Substitute.For<ILogger>(), CreateTelemetryClient());

            _userService.Create(arcUserDto, Arg.Any<string>()).Returns(createdUserDto);
            _credentialsFactory.Create(authorization).Returns(new Credentials("tenantId", "userLogin"));

            var result = await _controller.Create(arcUserDto, authorization);

            var createdAtResult = Assert.IsType<CreatedResult>(result);
            Assert.Equal(StatusCodes.Status201Created, createdAtResult.StatusCode);
            Assert.Equal(createdUserDto, createdAtResult.Value);
        }

        [Fact]
        public async Task Create_Success_RequestUser_Test()
        {
            var requestUser = "TestRequestUser";
            var arcUserDto = new ArcUserDto("email@test.com", "password", null, false, true, AuthenticationType.DatabaseUser, Language.Ingles)
            {
                Login = "newuser@example.com",
                Name = "New User"
            };

            var authorization = "Bearer valid_token";
            var createdUserDto = new OutputUserDto { Login = "newuser@example.com", Name = "New User" };

            var _userService = Substitute.For<IUserClientService>();
            var _credentialsFactory = Substitute.For<ICredentialsFactory>();
            var _controller = new UsersController(_userService, _credentialsFactory, Substitute.For<ILogger>(), CreateTelemetryClient());

            _userService.Create(arcUserDto, Arg.Any<string>()).Returns(createdUserDto);
            _credentialsFactory.Create(authorization, requestUser).Returns(new Credentials("tenantId", "userLogin"));

            var result = await _controller.Create(arcUserDto, authorization, requestUser);

            var createdAtResult = Assert.IsType<CreatedResult>(result);
            Assert.Equal(StatusCodes.Status201Created, createdAtResult.StatusCode);
            Assert.Equal(createdUserDto, createdAtResult.Value);
        }

        [Fact]
        public async Task Create_BadRequest_NullUser_Test()
        {
            var authorization = "Bearer valid_token";

            var _userService = Substitute.For<IUserClientService>();
            var _credentialsFactory = Substitute.For<ICredentialsFactory>();
            var _controller = new UsersController(_userService, _credentialsFactory, Substitute.For<ILogger>(), CreateTelemetryClient());

            _credentialsFactory.Create(authorization).Returns(new Credentials("tenantId", "userLogin"));

            var result = await _controller.Create((ArcUserDto)null, authorization);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task Create_NotFound_Test()
        {
            var arcUserDto = new ArcUserDto("email@test.com", "password", null, false, true, AuthenticationType.DatabaseUser, Language.Ingles)
            {
                Login = "nonexistent@example.com",
                Name = "Nonexistent User"
            };

            var authorization = "Bearer valid_token";

            var _userService = Substitute.For<IUserClientService>();
            var _credentialsFactory = Substitute.For<ICredentialsFactory>();
            var _controller = new UsersController(_userService, _credentialsFactory, Substitute.For<ILogger>(), CreateTelemetryClient());

            _userService.Create(arcUserDto, Arg.Any<string>()).Throws(new NotFoundAppException("User not found."));
            _credentialsFactory.Create(authorization).Returns(new Credentials("tenantId", "userLogin"));

            var result = await _controller.Create(arcUserDto, authorization);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task Create_Conflict_Test()
        {
            var arcUserDto = new ArcUserDto("email@test.com", "password", null, false, true, AuthenticationType.DatabaseUser, Language.Ingles)
            {
                Login = "nonexistent@example.com",
                Name = "Nonexistent User"
            };

            var authorization = "Bearer valid_token";

            var _userService = Substitute.For<IUserClientService>();
            var _credentialsFactory = Substitute.For<ICredentialsFactory>();
            var _controller = new UsersController(_userService, _credentialsFactory, Substitute.For<ILogger>(), CreateTelemetryClient());

            _userService.Create(arcUserDto, Arg.Any<string>()).Throws(new ConflictAppException("Conflict test."));
            _credentialsFactory.Create(authorization).Returns(new Credentials("tenantId", "userLogin"));

            var result = await _controller.Create(arcUserDto, authorization);

            var notFoundResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(StatusCodes.Status409Conflict, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task Create_BadRequest_Exception_Test()
        {
            var arcUserDto = new ArcUserDto("email@test.com", "password", null, false, true, AuthenticationType.DatabaseUser, Language.Ingles)
            {
                Login = "nonexistent@example.com",
                Name = "Nonexistent User"
            };

            var authorization = "Bearer valid_token";

            var _userService = Substitute.For<IUserClientService>();
            var _credentialsFactory = Substitute.For<ICredentialsFactory>();
            var _controller = new UsersController(_userService, _credentialsFactory, Substitute.For<ILogger>(), CreateTelemetryClient());

            _userService.Create(arcUserDto, Arg.Any<string>()).Throws(new AppException("App Exception test."));
            _credentialsFactory.Create(authorization).Returns(new Credentials("tenantId", "userLogin"));

            var result = await _controller.Create(arcUserDto, authorization);

            var notFoundResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task Create_InternalServerError_Test()
        {
            var arcUserDto = new ArcUserDto("email@test.com", "password", null, false, true, AuthenticationType.DatabaseUser, Language.Ingles)
            {
                Login = "error@example.com",
                Name = "Error User"
            };
            var authorization = "Bearer valid_token";
            var exception = new Exception("Internal server error");

            var _userService = Substitute.For<IUserClientService>();
            var _credentialsFactory = Substitute.For<ICredentialsFactory>();
            var _controller = new UsersController(_userService, _credentialsFactory, Substitute.For<ILogger>(), CreateTelemetryClient());

            _userService.Create(arcUserDto, Arg.Any<string>()).Throws(exception);
            _credentialsFactory.Create(authorization).Returns(new Credentials("tenantId", "userLogin"));

            var result = await _controller.Create(arcUserDto, authorization);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async Task AssociateToUserGroup_NoContentSuccess_Test()
        {
            var userId = "testUserId";
            var userGroupId = "testUserGroupId";
            var authorization = "Bearer valid_token";

            var _userService = Substitute.For<IUserClientService>();
            var _credentialsFactory = Substitute.For<ICredentialsFactory>();
            _credentialsFactory.Create(authorization).Returns(new Credentials("tenantId", "userLogin"));
            var _controller = new UsersController(_userService, _credentialsFactory, Substitute.For<ILogger>(), CreateTelemetryClient());

            _userService.AssociateToUserGroup(userId, userGroupId, Arg.Any<string>()).Returns(Task.CompletedTask);

            var result = await _controller.AssociateToUserGroup(userId, userGroupId, authorization);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task AssociateToUserGroup_BadRequest_Test()
        {
            var userId = "testUserId";
            var userGroupId = "testUserGroupId";
            var authorization = "Bearer valid_token";

            var _userService = Substitute.For<IUserClientService>();
            var _credentialsFactory = Substitute.For<ICredentialsFactory>();
            _credentialsFactory.Create(authorization).Returns(new Credentials("tenantId", "userLogin"));
            var _controller = new UsersController(_userService, _credentialsFactory, Substitute.For<ILogger>(), CreateTelemetryClient());

            _userService.AssociateToUserGroup(userId, userGroupId, Arg.Any<string>()).Throws(new AppException("Bad Request"));

            var result = await _controller.AssociateToUserGroup(userId, userGroupId, authorization);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task DissociateFromUserGroup_NoContentSuccess_Test()
        {
            var userId = "testUserId";
            var userGroupId = "testUserGroupId";
            var authorization = "Bearer valid_token";

            var _userService = Substitute.For<IUserClientService>();
            var _credentialsFactory = Substitute.For<ICredentialsFactory>();
            _credentialsFactory.Create(authorization).Returns(new Credentials("tenantId", "userLogin"));
            var _controller = new UsersController(_userService, _credentialsFactory, Substitute.For<ILogger>(), CreateTelemetryClient());

            _userService.DissociateFromUserGroup(userId, userGroupId, Arg.Any<string>()).Returns(Task.CompletedTask);

            var result = await _controller.DissociateFromUserGroup(userId, userGroupId, authorization);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DissociateFromUserGroup_NotFound_Test()
        {
            var userId = "testUserId";
            var userGroupId = "testUserGroupId";
            var authorization = "Bearer valid_token";

            var _userService = Substitute.For<IUserClientService>();
            var _credentialsFactory = Substitute.For<ICredentialsFactory>();
            _credentialsFactory.Create(authorization).Returns(new Credentials("tenantId", "userLogin"));
            var _controller = new UsersController(_userService, _credentialsFactory, Substitute.For<ILogger>(), CreateTelemetryClient());

            _userService.DissociateFromUserGroup(userId, userGroupId, Arg.Any<string>()).Throws(new NotFoundAppException("User or group not found"));

            var result = await _controller.DissociateFromUserGroup(userId, userGroupId, authorization);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task DissociateFromUserGroup_BadRequest_Test()
        {
            var userId = "testUserId";
            var userGroupId = "testUserGroupId";
            var authorization = "Bearer valid_token";

            var _userService = Substitute.For<IUserClientService>();
            var _credentialsFactory = Substitute.For<ICredentialsFactory>();
            _credentialsFactory.Create(authorization).Returns(new Credentials("tenantId", "userLogin"));
            var _controller = new UsersController(_userService, _credentialsFactory, Substitute.For<ILogger>(), CreateTelemetryClient());

            _userService.DissociateFromUserGroup(userId, userGroupId, Arg.Any<string>()).Throws(new AppException("Bad Request"));

            var result = await _controller.DissociateFromUserGroup(userId, userGroupId, authorization);

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
