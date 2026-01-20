using AutoMapper;
using MassTransit;
using Microsoft.Data.SqlClient;
using Moq;
using Identidade.Dominio.Helpers;
using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Modelos;
using Identidade.Dominio.Servicos;
using Identidade.Infraestrutura.Helpers;
using Identidade.Publico.Dtos;
using Identidade.Publico.Enumerations;
using Identidade.Publico.Events;
using Identidade.UnitTests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;
using ValidationResult = FluentValidation.Results.ValidationResult;
using Identidade.Infraestrutura.Configuracoes;
using Identidade.Infraestrutura.ServicosCliente;
using Identidade.Dominio.Repositorios;

namespace Identidade.UnitTests.Infraestrutura.ServicosCliente
{
    public class UserClientServiceTests
    {
        public static IEnumerable<object[]> GetUserClientServiceConstructorTestParameters()
        {
            return ParameterTestHelper.GetParameters(s => s
                .AddNullableParameter("userRepository", Mock.Of<IUserRepository>())
                .AddNullableParameter("authorizationService", Mock.Of<IAuthorizationService>())
                .AddNullableParameter("mapper", Mock.Of<IMapper>())
                .AddNullableParameter("userValidator", Mock.Of<IUserValidator>())
                .AddNullableParameter("passwordValidator", Mock.Of<IPasswordValidator>())
                .AddNullableParameter("bus", Mock.Of<IBus>())
                .AddNullableParameter("settings", Mock.Of<ISettings>())
                .AddNullableParameter("databaseConnectionModifier", Mock.Of<IDatabaseConnectionUserModifier>())
                .AddNullableParameter("arcUserXmlWriter", Mock.Of<IArcUserXmlWriter>()));
        }

        [Theory]
        [MemberData(nameof(GetUserClientServiceConstructorTestParameters))]
        public void UserClientServiceConstructorTest(IUserRepository userRepository, IAuthorizationService authorizationService, IMapper mapper,
            IUserValidator userValidator, IPasswordValidator passwordValidator, IBus bus, ISettings settings, IDatabaseConnectionUserModifier databaseConnectionModifier, IArcUserXmlWriter arcUserXmlWriter,
            string missingParameterName = null)
        {
            UserClientService Create() => new UserClientService(userRepository, authorizationService, mapper,
                userValidator, passwordValidator, bus, settings, databaseConnectionModifier, arcUserXmlWriter);

            if (string.IsNullOrWhiteSpace(missingParameterName))
            {
                var userClientService = Create();

                Assert.NotNull(userClientService);
            }
            else
            {
                var ex = Assert.Throws<ArgumentNullException>(Create);
                Assert.Equal(missingParameterName, ex.ParamName);
            }
        }

        [Fact]
        public async Task CreateUserClientService()
        {
            var inputUserDto = new InputUserDto
            {
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient",
                Login = "login",
                PasswordExpiration = DateTime.Now,
                ArcXml = ""
            };

            var outputUserDto = new OutputUserDto
            {
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient",
                Login = "login",
                PasswordExpiration = DateTime.Now

            };

            var user = new User
            {
                Name = "User"
            };

            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();

            mapperMoq.Setup(s => s.Map<InputUserDto, User>(It.IsAny<InputUserDto>())).Returns(user);
            mapperMoq.Setup(s => s.Map<User, OutputUserDto>(It.IsAny<User>())).Returns(outputUserDto);

            SetMockConfigureParametersToCreateUpdate(userRepositoryMoq);

            userRepositoryMoq.Setup(s => s.Create(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(user);

            userRepositoryMoq.Setup(s => s.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(true);

            var userClientServiceMoq = new UserClientService(userRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object,
                userValidatorMoq.Object, passwordValidatorMoq.Object, busMoq.Object, settingsMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>(), Mock.Of<IArcUserXmlWriter>());

            var createdUser = await userClientServiceMoq.Create(inputUserDto, "", null);

            Assert.Equal(createdUser.Name, inputUserDto.Name);
            Assert.NotNull(createdUser);
        }

        [Fact]
        public async Task CreateUserClientService_NullUserGroupsAndSubstituteUsers()
        {
            var inputUserDto = new InputUserDto
            {
                SubstituteUsers = null,
                UserGroups = null,
                Name = "UserClient",
                Login = "login",
                PasswordExpiration = DateTime.Now,
                ArcXml = ""
            };

            var outputUserDto = new OutputUserDto
            {
                SubstituteUsers = null,
                UserGroups = null,
                Name = "UserClient",
                Login = "login",
                PasswordExpiration = DateTime.Now

            };

            var user = new User
            {
                Name = "User"
            };

            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();

            mapperMoq.Setup(s => s.Map<InputUserDto, User>(It.IsAny<InputUserDto>())).Returns(user);
            mapperMoq.Setup(s => s.Map<User, OutputUserDto>(It.IsAny<User>())).Returns(outputUserDto);

            SetMockConfigureParametersToCreateUpdate(userRepositoryMoq);

            userRepositoryMoq.Setup(s => s.Create(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(user);

            userRepositoryMoq.Setup(s => s.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(true);

            var userClientServiceMoq = new UserClientService(userRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object,
                userValidatorMoq.Object, passwordValidatorMoq.Object, busMoq.Object, settingsMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>(), Mock.Of<IArcUserXmlWriter>());

            var createdUser = await userClientServiceMoq.Create(inputUserDto, "", null);

            Assert.Equal(createdUser.Name, inputUserDto.Name);
            Assert.NotNull(createdUser);
            userValidatorMoq.Verify(p => p.VerifyExistences(null, null), Times.Once);
        }

        [Fact]
        public async Task CreateUserClientServiceParametersValidation()
        {
            var inputUserDto = new InputUserDto
            {
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient",
                Login = "login",
                PasswordExpiration = DateTime.Now,
                Active = true,
                AuthenticationType = null,
                Password = "teste",
                ArcXml = ""
            };

            var outputUserDto = new OutputUserDto
            {
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient",
                Login = "login",
                PasswordExpiration = DateTime.Now

            };

            var user = new User
            {
                Name = "User"
            };

            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();

            mapperMoq.Setup(s => s.Map<InputUserDto, User>(It.IsAny<InputUserDto>())).Returns(user);
            mapperMoq.Setup(s => s.Map<User, OutputUserDto>(It.IsAny<User>())).Returns(outputUserDto);
            settingsMoq.Setup(s => s.AuthenticationType).Returns(SettingsAuthenticationType.User);

            SetMockConfigureParametersToCreateUpdate(userRepositoryMoq);

            userRepositoryMoq.Setup(s => s.Create(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(user);

            userRepositoryMoq.Setup(s => s.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(true);

            var userClientServiceMoq = new UserClientService(userRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object,
                userValidatorMoq.Object, passwordValidatorMoq.Object, busMoq.Object, settingsMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>(), Mock.Of<IArcUserXmlWriter>());

            var createdUser = await userClientServiceMoq.Create(inputUserDto, "", null);

            Assert.Equal(createdUser.Name, inputUserDto.Name);
            Assert.Null(createdUser.AuthenticationType);
            Assert.NotNull(createdUser);
        }

        [Fact]
        public void CreateUserClientService_AzureAdValidation()
        {
            var expectedMessage = $"Email must be provided for authentication type {AuthenticationType.AzureAD}";
            var inputUserDto = new InputUserDto
            {
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient",
                Login = "login",
                PasswordExpiration = DateTime.Now,
                Active = true,
                AuthenticationType = AuthenticationType.AzureAD,
                Password = "teste",
                ArcXml = ""
            };

            var user = new User
            {
                Name = "User"
            };

            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();

            mapperMoq.Setup(s => s.Map<InputUserDto, User>(It.IsAny<InputUserDto>())).Returns(user);
            settingsMoq.Setup(s => s.AuthenticationType).Returns(SettingsAuthenticationType.User);

            var userClientServiceMoq = new UserClientService(userRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object,
                Mock.Of<IUserValidator>(), passwordValidatorMoq.Object, busMoq.Object, settingsMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>(), Mock.Of<IArcUserXmlWriter>());

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await userClientServiceMoq.Create(inputUserDto, "", null));
            Assert.Equal(expectedMessage, ex.Result.Message);
        }

        public static IEnumerable<object[]> GetCreateFromArcUserDtoParameters()
        {
            var arcUserDto = new ArcUserDto("TestEmail@test.com", "TestPassword", new DateTimeOffset(2023, 04, 18, 1, 2, 3, TimeSpan.FromHours(3)).ToString(), false, true, AuthenticationType.DatabaseUser, Language.Portugues)
            {
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient",
                Login = "login"
            };

            var arcUserDtoAzureAd = new ArcUserDto("TestEmail@test.com", string.Empty, new DateTimeOffset(2023, 04, 18, 1, 2, 3, TimeSpan.FromHours(3)).ToString(), false, true, AuthenticationType.AzureAD, Language.Portugues)
            {
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient",
                Login = "login"
            };

            var arcUserDtoActiveDirectory = new ArcUserDto("TestEmail@test.com", string.Empty, new DateTimeOffset(2023, 04, 18, 1, 2, 3, TimeSpan.FromHours(3)).ToString(), false, true, AuthenticationType.ActiveDirectory, Language.Portugues)
            {
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient",
                Login = "login"
            };

            yield return new object[] { arcUserDto };
            yield return new object[] { arcUserDtoAzureAd };
            yield return new object[] { arcUserDtoActiveDirectory };
        }

        [Theory]
        [MemberData(nameof(GetCreateFromArcUserDtoParameters))]
        public async Task CreateFromArcUserUserClientServiceTest(ArcUserDto arcUserDto)
        {
            var arcUser = new ArcUser("login", "UserClient", "TestEmail@test.com", "TestPassword", Language.Portugues.ToString(), null, null, AuthenticationType.DatabaseUser.ToString(), new DateTimeOffset(2023, 04, 18, 1, 2, 3, TimeSpan.FromHours(3)), false, true);

            var outputUserDto = new OutputUserDto
            {
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient",
                Login = "login",
                PasswordExpiration = DateTime.Now
            };

            var user = new User
            {
                Name = "User",
                PasswordHistory = "testHistory"
            };

            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();
            var arcUserXmlWriteMoq = new Mock<IArcUserXmlWriter>();

            mapperMoq.Setup(s => s.Map<User>(It.IsAny<ArcUserDto>())).Returns(user);
            mapperMoq.Setup(s => s.Map<ArcUser>(It.IsAny<ArcUserDto>())).Returns(arcUser);
            mapperMoq.Setup(s => s.Map<OutputUserDto>(It.IsAny<User>())).Returns(outputUserDto);
            settingsMoq.Setup(s => s.AuthenticationType).Returns(SettingsAuthenticationType.User);

            SetMockConfigureParametersToCreateUpdate(userRepositoryMoq);

            userRepositoryMoq.Setup(s => s.Create(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(user);

            userRepositoryMoq.Setup(s => s.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(true);

            arcUserXmlWriteMoq.Setup(s => s.Write(arcUser, It.IsAny<string>(), user.PasswordHistory)).Returns(new XElement("root"));

            var userClientServiceMoq = new UserClientService(userRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object,
                userValidatorMoq.Object, passwordValidatorMoq.Object, busMoq.Object, settingsMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>(), arcUserXmlWriteMoq.Object);

            var createdUser = await userClientServiceMoq.Create(arcUserDto, "", null);

            Assert.NotNull(createdUser);
            Assert.Equal(createdUser.Name, outputUserDto.Name);
            Assert.Equal(createdUser.Login, outputUserDto.Login);
            Assert.Equal(createdUser.PasswordExpiration, outputUserDto.PasswordExpiration);
        }

        [Fact]
        public Task CreateFromArcUserUserClientService_NullCreatedUserTest()
        {
            var arcUser = new ArcUserDto("TestEmail@test.com", "TestPassword", new DateTimeOffset(2023, 04, 18, 1, 2, 3, TimeSpan.FromHours(3)).ToString(), false, true, AuthenticationType.DatabaseUser, Language.Portugues)
            {
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient",
                Login = "login"
            };

            var outputUserDto = new OutputUserDto
            {
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient",
                Login = "login",
                PasswordExpiration = DateTime.Now

            };

            var user = new User
            {
                Name = "User"
            };

            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();

            mapperMoq.Setup(s => s.Map<User>(It.IsAny<ArcUserDto>())).Returns(user);
            mapperMoq.Setup(s => s.Map<OutputUserDto>(It.IsAny<User>())).Returns(outputUserDto);
            settingsMoq.Setup(s => s.AuthenticationType).Returns(SettingsAuthenticationType.User);

            userRepositoryMoq.Setup(s => s.Create(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync((User)null);

            var userClientServiceMoq = new UserClientService(userRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object,
                userValidatorMoq.Object, passwordValidatorMoq.Object, busMoq.Object, settingsMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>(), Mock.Of<IArcUserXmlWriter>());

            var ex = Assert.ThrowsAsync<AppException>(async () => await userClientServiceMoq.Create(arcUser, string.Empty, null));
            Assert.Equal(Constants.Exception.cst_NullUser, ex.Result.Message);
            return Task.CompletedTask;
        }

        [Fact]
        public Task CreateFromArcUserUserClientService_NotSucessErrorCreateArcTest()
        {
            var expectedError = "TestError";
            var expectedMessage = $"{Constants.Exception.cst_UserCreationFailed} Error: '{expectedError}'";
            var arcUserDto = new ArcUserDto("TestEmail@test.com", "TestPassword", new DateTimeOffset(2023, 04, 18, 1, 2, 3, TimeSpan.FromHours(3)).ToString(), false, true, AuthenticationType.DatabaseUser, Language.Portugues)
            {
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient",
                Login = "login"
            };

            var arcUser = new ArcUser("login", "UserClient", "TestEmail@test.com", "TestPassword", Language.Portugues.ToString(), null, null, AuthenticationType.DatabaseUser.ToString(), new DateTimeOffset(2023, 04, 18, 1, 2, 3, TimeSpan.FromHours(3)), false, true);

            var outputUserDto = new OutputUserDto
            {
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient",
                Login = "login",
                PasswordExpiration = DateTime.Now

            };

            var user = new User
            {
                Name = "User",
                PasswordHistory = "testHistory"
            };

            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();
            var arcUserXmlWriteMoq = new Mock<IArcUserXmlWriter>();

            mapperMoq.Setup(s => s.Map<User>(It.IsAny<ArcUserDto>())).Returns(user);
            mapperMoq.Setup(s => s.Map<ArcUser>(It.IsAny<ArcUserDto>())).Returns(arcUser);
            mapperMoq.Setup(s => s.Map<OutputUserDto>(It.IsAny<User>())).Returns(outputUserDto);
            settingsMoq.Setup(s => s.AuthenticationType).Returns(SettingsAuthenticationType.User);

            var sqlParameters = new List<SqlParameter>
            {
                new SqlParameter(Constants.cst_Erro, expectedError)
            };
            SetMockConfigureParametersToCreateUpdate(userRepositoryMoq, sqlParameters);

            userRepositoryMoq.Setup(s => s.Create(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(user);

            userRepositoryMoq.Setup(s => s.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(false);

            arcUserXmlWriteMoq.Setup(s => s.Write(arcUser, It.IsAny<string>(), user.PasswordHistory)).Returns(new XElement("root"));

            var userClientServiceMoq = new UserClientService(userRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object,
                userValidatorMoq.Object, passwordValidatorMoq.Object, busMoq.Object, settingsMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>(), arcUserXmlWriteMoq.Object);

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await userClientServiceMoq.Create(arcUserDto, string.Empty, null));
            Assert.Equal(expectedMessage, ex.Result.Message);
            return Task.CompletedTask;
        }

        [Fact]
        public Task CreateFromArcUserUserClientService_NotSucessNoChangeCreateArcTest()
        {
            var arcUserDto = new ArcUserDto("TestEmail@test.com", "TestPassword", new DateTimeOffset(2023, 04, 18, 1, 2, 3, TimeSpan.FromHours(3)).ToString(), false, true, AuthenticationType.DatabaseUser, Language.Portugues)
            {
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient",
                Login = "login"
            };

            var arcUser = new ArcUser("login", "UserClient", "TestEmail@test.com", "TestPassword", Language.Portugues.ToString(), null, null, AuthenticationType.DatabaseUser.ToString(), new DateTimeOffset(2023, 04, 18, 1, 2, 3, TimeSpan.FromHours(3)), false, true);

            var outputUserDto = new OutputUserDto
            {
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient",
                Login = "login",
                PasswordExpiration = DateTime.Now

            };

            var user = new User
            {
                Name = "User",
                PasswordHistory = "testHistory"
            };

            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();
            var arcUserXmlWriteMoq = new Mock<IArcUserXmlWriter>();

            mapperMoq.Setup(s => s.Map<User>(It.IsAny<ArcUserDto>())).Returns(user);
            mapperMoq.Setup(s => s.Map<ArcUser>(It.IsAny<ArcUserDto>())).Returns(arcUser);
            mapperMoq.Setup(s => s.Map<OutputUserDto>(It.IsAny<User>())).Returns(outputUserDto);
            settingsMoq.Setup(s => s.AuthenticationType).Returns(SettingsAuthenticationType.User);

            var sqlParameters = new List<SqlParameter>
            {
                new SqlParameter(Constants.cst_FoiAlterado, false)
            };
            SetMockConfigureParametersToCreateUpdate(userRepositoryMoq, sqlParameters);

            userRepositoryMoq.Setup(s => s.Create(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(user);

            userRepositoryMoq.Setup(s => s.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(false);

            arcUserXmlWriteMoq.Setup(s => s.Write(arcUser, It.IsAny<string>(), user.PasswordHistory)).Returns(new XElement("root"));

            var userClientServiceMoq = new UserClientService(userRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object,
                userValidatorMoq.Object, passwordValidatorMoq.Object, busMoq.Object, settingsMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>(), arcUserXmlWriteMoq.Object);

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await userClientServiceMoq.Create(arcUserDto, string.Empty, null));
            Assert.Equal(Constants.Exception.cst_UserCreationFailed, ex.Result.Message);
            return Task.CompletedTask;
        }

        public static IEnumerable<object[]> GetCreateFromArcUserUserClientServiceArcUserValidationParameters()
        {
            var invalidAzureAdUser = new ArcUserDto("TestEmail@test.com", string.Empty, new DateTimeOffset(2023, 04, 18, 1, 2, 3, TimeSpan.FromHours(3)).ToString(), false, true, AuthenticationType.AzureAD, Language.Portugues)
            {
                SubstituteUsers = new string[] { },
                UserGroups = new string[] { },
                Name = "UserClient",
                Login = "login"
            };
            invalidAzureAdUser.Email = string.Empty;

            var invalidPasswordUser = new ArcUserDto(string.Empty, "TestPassword", new DateTimeOffset(2023, 04, 18, 1, 2, 3, TimeSpan.FromHours(3)).ToString(), false, true, AuthenticationType.DatabaseUser, Language.Portugues)
            {
                SubstituteUsers = new string[] { },
                UserGroups = new string[] { },
                Name = "UserClient",
                Login = "login"
            };


            yield return new object[] { null, new InvalidOperationException(Constants.Exception.cst_NullUser) };
            yield return new object[] { invalidAzureAdUser, new InvalidOperationException($"Email must be provided for authentication type {AuthenticationType.AzureAD}") };
            yield return new object[] { invalidPasswordUser, new InvalidOperationException("Invalid Password"), false };

            yield return new object[] { new ArcUserDto(string.Empty, "TestPassword", new DateTimeOffset(2023, 04, 18, 1, 2, 3, TimeSpan.FromHours(3)).ToString(), false, true, AuthenticationType.DatabaseUser, Language.Portugues) { Login = null }, new ArgumentException(Constants.Exception.cst_InvalidLogin) };
            yield return new object[] { new ArcUserDto(string.Empty, "TestPassword", new DateTimeOffset(2023, 04, 18, 1, 2, 3, TimeSpan.FromHours(3)).ToString(), false, true, AuthenticationType.DatabaseUser, Language.Portugues) { Login = string.Empty }, new ArgumentException(Constants.Exception.cst_InvalidLogin) };
            yield return new object[] { new ArcUserDto(string.Empty, "TestPassword", new DateTimeOffset(2023, 04, 18, 1, 2, 3, TimeSpan.FromHours(3)).ToString(), false, true, AuthenticationType.DatabaseUser, Language.Portugues) { Login = " " }, new ArgumentException(Constants.Exception.cst_InvalidLogin) };
        }

        [Theory]
        [MemberData(nameof(GetCreateFromArcUserUserClientServiceArcUserValidationParameters))]
        public void CreateFromArcUserUserClientService_ValidationTest(ArcUserDto arcUser, Exception expectedException, bool validPassword = true)
        {
            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();

            if (!validPassword)
                passwordValidatorMoq.Setup(s => s.Validate(It.IsAny<string>())).Throws(expectedException);

            var userClientServiceMoq = new UserClientService(userRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object,
                Mock.Of<IUserValidator>(), passwordValidatorMoq.Object, busMoq.Object, settingsMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>(), Mock.Of<IArcUserXmlWriter>());

            var ex = Assert.ThrowsAsync(expectedException.GetType(), async () => await userClientServiceMoq.Create(arcUser, string.Empty, null));
            Assert.Equal(expectedException.Message, ex.Result.Message);
        }

        public static IEnumerable<object[]> GetUpdateFromArcUserUserClientServiceArcUserValidationParameters()
        {
            var invalidAzureAdUser = new ArcUserDto("TestEmail@test.com", string.Empty, new DateTimeOffset(2023, 04, 18, 1, 2, 3, TimeSpan.FromHours(3)).ToString(), false, true, AuthenticationType.AzureAD, Language.Portugues)
            {
                SubstituteUsers = new string[] { },
                UserGroups = new string[] { },
                Name = "UserClient",
                Login = "login"
            };
            invalidAzureAdUser.Email = string.Empty;

            var invalidPasswordUser = new ArcUserDto(string.Empty, "TestPassword", new DateTimeOffset(2023, 04, 18, 1, 2, 3, TimeSpan.FromHours(3)).ToString(), false, true, AuthenticationType.DatabaseUser, Language.Portugues)
            {
                SubstituteUsers = new string[] { },
                UserGroups = new string[] { },
                Name = "UserClient",
                Login = "login"
            };


            yield return new object[] { null, new InvalidOperationException(Constants.Exception.cst_NullUser) };
            yield return new object[] { invalidAzureAdUser, new InvalidOperationException($"Email must be provided for authentication type {AuthenticationType.AzureAD}") };
            yield return new object[] { invalidPasswordUser, new InvalidOperationException("Invalid Password"), false };
        }

        [Fact]
        public async Task UpdateFromArcUserUserClientServiceTest()
        {
            var arcUserDto = new ArcUserDto("TestEmail@test.com", "TestPassword", new DateTimeOffset(2023, 04, 18, 1, 2, 3, TimeSpan.FromHours(3)).ToString(), false, true, AuthenticationType.DatabaseUser, Language.Portugues)
            {
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient",
                Login = "login"
            };

            var arcUser = new ArcUser("login", "UserClient", "TestEmail@test.com", "TestPassword", Language.Portugues.ToString(), null, null, AuthenticationType.DatabaseUser.ToString(), new DateTimeOffset(2023, 04, 18, 1, 2, 3, TimeSpan.FromHours(3)), false, true);

            var outputUserDto = new OutputUserDto
            {
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient",
                Login = "login",
                PasswordExpiration = DateTime.Now
            };

            var user = new User
            {
                Name = "User",
                PasswordHistory = "testHistory"
            };

            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();
            var arcUserXmlWriteMoq = new Mock<IArcUserXmlWriter>();

            mapperMoq.Setup(s => s.Map<User>(It.IsAny<ArcUserDto>())).Returns(user);
            mapperMoq.Setup(s => s.Map<ArcUser>(It.IsAny<ArcUserDto>())).Returns(arcUser);
            mapperMoq.Setup(s => s.Map<OutputUserDto>(It.IsAny<User>())).Returns(outputUserDto);
            settingsMoq.Setup(s => s.AuthenticationType).Returns(SettingsAuthenticationType.User);

            SetMockConfigureParametersToCreateUpdate(userRepositoryMoq);

            userRepositoryMoq.Setup(s => s.Update(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(user);

            userRepositoryMoq.Setup(s => s.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(true);

            arcUserXmlWriteMoq.Setup(s => s.Write(arcUser, It.IsAny<string>(), user.PasswordHistory)).Returns(new XElement("root"));

            var userClientServiceMoq = new UserClientService(userRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object,
                userValidatorMoq.Object, passwordValidatorMoq.Object, busMoq.Object, settingsMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>(), arcUserXmlWriteMoq.Object);

            var updatedUser = await userClientServiceMoq.Update(string.Empty, arcUserDto, null);

            Assert.NotNull(updatedUser);
            Assert.Equal(updatedUser.Name, outputUserDto.Name);
            Assert.Equal(updatedUser.Login, outputUserDto.Login);
            Assert.Equal(updatedUser.PasswordExpiration, outputUserDto.PasswordExpiration);
        }

        [Fact]
        public Task UpdateFromArcUserUserClientService_NullCreatedUserTest()
        {
            var arcUser = new ArcUserDto("TestEmail@test.com", "TestPassword", new DateTimeOffset(2023, 04, 18, 1, 2, 3, TimeSpan.FromHours(3)).ToString(), false, true, AuthenticationType.DatabaseUser, Language.Portugues)
            {
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient",
                Login = "login"
            };

            var outputUserDto = new OutputUserDto
            {
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient",
                Login = "login",
                PasswordExpiration = DateTime.Now

            };

            var user = new User
            {
                Name = "User"
            };

            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();

            mapperMoq.Setup(s => s.Map<User>(It.IsAny<ArcUserDto>())).Returns(user);
            mapperMoq.Setup(s => s.Map<OutputUserDto>(It.IsAny<User>())).Returns(outputUserDto);
            settingsMoq.Setup(s => s.AuthenticationType).Returns(SettingsAuthenticationType.User);

            userRepositoryMoq.Setup(s => s.Update(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync((User)null);

            var userClientServiceMoq = new UserClientService(userRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object,
                userValidatorMoq.Object, passwordValidatorMoq.Object, busMoq.Object, settingsMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>(), Mock.Of<IArcUserXmlWriter>());

            var ex = Assert.ThrowsAsync<AppException>(async () => await userClientServiceMoq.Update(string.Empty, arcUser, null));
            Assert.Equal(Constants.Exception.cst_NullUser, ex.Result.Message);
            return Task.CompletedTask;
        }

        [Fact]
        public Task UpdateFromArcUserUserClientService_NotSucessErrorUpdateArcTest()
        {
            var expectedError = "TestError";
            var expectedMessage = $"{Constants.Exception.cst_UserUpdateFailed} Error: '{expectedError}'";
            var arcUserDto = new ArcUserDto("TestEmail@test.com", "TestPassword", new DateTimeOffset(2023, 04, 18, 1, 2, 3, TimeSpan.FromHours(3)).ToString(), false, true, AuthenticationType.DatabaseUser, Language.Portugues)
            {
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient",
                Login = "login"
            };

            var arcUser = new ArcUser("login", "UserClient", "TestEmail@test.com", "TestPassword", Language.Portugues.ToString(), null, null, AuthenticationType.DatabaseUser.ToString(), new DateTimeOffset(2023, 04, 18, 1, 2, 3, TimeSpan.FromHours(3)), false, true);

            var outputUserDto = new OutputUserDto
            {
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient",
                Login = "login",
                PasswordExpiration = DateTime.Now

            };

            var user = new User
            {
                Name = "User",
                PasswordHistory = "testHistory"
            };

            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();
            var arcUserXmlWriteMoq = new Mock<IArcUserXmlWriter>();

            mapperMoq.Setup(s => s.Map<User>(It.IsAny<ArcUserDto>())).Returns(user);
            mapperMoq.Setup(s => s.Map<ArcUser>(It.IsAny<ArcUserDto>())).Returns(arcUser);
            mapperMoq.Setup(s => s.Map<OutputUserDto>(It.IsAny<User>())).Returns(outputUserDto);
            settingsMoq.Setup(s => s.AuthenticationType).Returns(SettingsAuthenticationType.User);

            var sqlParameters = new List<SqlParameter>
            {
                new SqlParameter(Constants.cst_Erro, expectedError),
            };
            SetMockConfigureParametersToCreateUpdate(userRepositoryMoq, sqlParameters);

            userRepositoryMoq.Setup(s => s.Update(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(user);

            userRepositoryMoq.Setup(s => s.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(false);

            arcUserXmlWriteMoq.Setup(s => s.Write(arcUser, It.IsAny<string>(), user.PasswordHistory)).Returns(new XElement("root"));

            var userClientServiceMoq = new UserClientService(userRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object,
                userValidatorMoq.Object, passwordValidatorMoq.Object, busMoq.Object, settingsMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>(), arcUserXmlWriteMoq.Object);

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await userClientServiceMoq.Update(string.Empty, arcUserDto, null));
            Assert.Equal(expectedMessage, ex.Result.Message);
            return Task.CompletedTask;
        }

        [Fact]
        public Task UpdateFromArcUserUserClientService_NotSucessNoChangeUpdateArcTest()
        {
            var arcUserDto = new ArcUserDto("TestEmail@test.com", "TestPassword", new DateTimeOffset(2023, 04, 18, 1, 2, 3, TimeSpan.FromHours(3)).ToString(), false, true, AuthenticationType.DatabaseUser, Language.Portugues)
            {
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient",
                Login = "login"
            };

            var arcUser = new ArcUser("login", "UserClient", "TestEmail@test.com", "TestPassword", Language.Portugues.ToString(), null, null, AuthenticationType.DatabaseUser.ToString(), new DateTimeOffset(2023, 04, 18, 1, 2, 3, TimeSpan.FromHours(3)), false, true);

            var outputUserDto = new OutputUserDto
            {
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient",
                Login = "login",
                PasswordExpiration = DateTime.Now

            };

            var user = new User
            {
                Name = "User",
                PasswordHistory = "testHistory"
            };

            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();
            var arcUserXmlWriteMoq = new Mock<IArcUserXmlWriter>();

            mapperMoq.Setup(s => s.Map<User>(It.IsAny<ArcUserDto>())).Returns(user);
            mapperMoq.Setup(s => s.Map<ArcUser>(It.IsAny<ArcUserDto>())).Returns(arcUser);
            mapperMoq.Setup(s => s.Map<OutputUserDto>(It.IsAny<User>())).Returns(outputUserDto);
            settingsMoq.Setup(s => s.AuthenticationType).Returns(SettingsAuthenticationType.User);

            var sqlParameters = new List<SqlParameter>
            {
                new SqlParameter(Constants.cst_FoiAlterado, false),
            };
            SetMockConfigureParametersToCreateUpdate(userRepositoryMoq, sqlParameters);

            userRepositoryMoq.Setup(s => s.Update(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(user);

            userRepositoryMoq.Setup(s => s.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(false);

            arcUserXmlWriteMoq.Setup(s => s.Write(arcUser, It.IsAny<string>(), user.PasswordHistory)).Returns(new XElement("root"));

            var userClientServiceMoq = new UserClientService(userRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object,
                userValidatorMoq.Object, passwordValidatorMoq.Object, busMoq.Object, settingsMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>(), arcUserXmlWriteMoq.Object);

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await userClientServiceMoq.Update(string.Empty, arcUserDto, null));
            Assert.Equal(Constants.Exception.cst_UserUpdateFailedNoChange, ex.Result.Message);
            return Task.CompletedTask;
        }

        [Theory]
        [MemberData(nameof(GetUpdateFromArcUserUserClientServiceArcUserValidationParameters))]
        public void UpdateFromArcUserUserClientService_ValidationTest(ArcUserDto arcUser, Exception expectedException, bool validPassword = true)
        {
            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();

            if (!validPassword)
                passwordValidatorMoq.Setup(s => s.Validate(It.IsAny<string>())).Throws(expectedException);

            var userClientServiceMoq = new UserClientService(userRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object,
                Mock.Of<IUserValidator>(), passwordValidatorMoq.Object, busMoq.Object, settingsMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>(), Mock.Of<IArcUserXmlWriter>());

            var ex = Assert.ThrowsAsync(expectedException.GetType(), async () => await userClientServiceMoq.Update(string.Empty, arcUser, null));
            Assert.Equal(expectedException.Message, ex.Result.Message);
        }

        [Fact]
        public async Task DeleteUserClient()
        {
            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();

            userRepositoryMoq.Setup(s => s.ConfigureParametersToRemove(out It.Ref<List<SqlParameter>>.IsAny, It.IsAny<string>()));

            userRepositoryMoq.Setup(s => s.Remove(It.IsAny<string>())).ReturnsAsync(true);

            userRepositoryMoq.Setup(s => s.RemoveItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(true);

            var userClientServiceMoq = new UserClientService(userRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object,
                userValidatorMoq.Object, passwordValidatorMoq.Object, busMoq.Object, settingsMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>(), Mock.Of<IArcUserXmlWriter>());

            await userClientServiceMoq.Delete("", null);

            userRepositoryMoq.Verify(v => v.Remove(It.IsAny<string>()), Times.Once());
            userRepositoryMoq.Verify(v => v.ConfigureParametersToRemove(out It.Ref<List<SqlParameter>>.IsAny, It.IsAny<string>()), Times.Once());
            userRepositoryMoq.Verify(v => v.RemoveItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>()), Times.Once());
        }
        [Fact]
        public void GetIReadOnlyCollectionOutputUserDtoGetAll()
        {
            var user1 = new User { Id = "USR_GUID1", UserName = "user1" };
            var user2 = new User { Id = "USR_GUID2", UserName = "user2" };

            var outputUserDto1 = new OutputUserDto
            {
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient1",
                Login = "login1",
                PasswordExpiration = DateTime.Now

            };

            var outputUserDto2 = new OutputUserDto
            {
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient2",
                Login = "login2",
                PasswordExpiration = DateTime.Now

            };

            var allUsers = new List<User> { user1, user2 };
            var allUsersDto = new List<OutputUserDto> { outputUserDto1, outputUserDto2 };

            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();

            mapperMoq.Setup(s => s.Map<IReadOnlyCollection<User>, IReadOnlyCollection<OutputUserDto>>(allUsers)).Returns(allUsersDto);

            var userClientServiceMoq = new UserClientService(userRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object,
                userValidatorMoq.Object, passwordValidatorMoq.Object, busMoq.Object, settingsMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>(), Mock.Of<IArcUserXmlWriter>());

            userRepositoryMoq.Setup(s => s.GetAll()).ReturnsAsync(allUsers);

            var retornoGetAll = userClientServiceMoq.Get(null);

            Assert.NotNull(retornoGetAll);
            Assert.Equal(retornoGetAll.Result.Count, allUsers.Count);
        }
        [Fact]
        public void GetIReadOnlyCollectionOutputUserDtoGetByLogin()
        {
            var user = new User { Id = "USR_GUID1", UserName = "UserClient1" };

            var outputUserDto = new OutputUserDto
            {
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient1",
                Login = "login1",
                PasswordExpiration = DateTime.Now

            };

            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();

            mapperMoq.Setup(s => s.Map<User, OutputUserDto>(It.IsAny<User>())).Returns(outputUserDto);

            var userClientServiceMoq = new UserClientService(userRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object,
                userValidatorMoq.Object, passwordValidatorMoq.Object, busMoq.Object, settingsMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>(), Mock.Of<IArcUserXmlWriter>());

            userRepositoryMoq.Setup(s => s.GetByName(It.IsAny<string>())).ReturnsAsync(user);

            var retornoGetByName = userClientServiceMoq.Get("login1");

            Assert.NotNull(retornoGetByName);
            Assert.Equal("UserClient1", retornoGetByName.Result.Select(s => s.Name).FirstOrDefault());
        }

        [Fact]
        public void GetById()
        {
            var user = new User { Id = "USR_GUID1", UserName = "UserClient1" };

            var outputUserDto = new OutputUserDto
            {
                Id = "USR_GUID1",
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient1",
                Login = "login1",
                PasswordExpiration = DateTime.Now

            };

            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();

            mapperMoq.Setup(s => s.Map<User, OutputUserDto>(It.IsAny<User>())).Returns(outputUserDto);

            var userClientServiceMoq = new UserClientService(userRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object,
                userValidatorMoq.Object, passwordValidatorMoq.Object, busMoq.Object, settingsMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>(), Mock.Of<IArcUserXmlWriter>());

            userRepositoryMoq.Setup(s => s.GetById(It.IsAny<string>())).ReturnsAsync(user);

            var retornoGetById = userClientServiceMoq.GetById("USR_GUID1");

            Assert.NotNull(retornoGetById);
            Assert.Equal("USR_GUID1", retornoGetById.Result.Id);
        }

        [Fact]
        public void GetUserGroups()
        {
            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();

            var user1 = new User { Id = "User1", Name = "New User1", UserName = "new.user1", Email = "new.user1@email.com" };
            var userGroup1 = new UserGroup { Id = "Group1", Name = "New Group1" };
            var userGroupUser1 = new UserGroupUser(userGroup1, user1);

            var user2 = new User { Id = "User1", Name = "New User1", UserName = "new.user2", Email = "new.user2@email.com" };
            var userGroup2 = new UserGroup { Id = "Group1", Name = "New Group1" };
            var userGroupUser2 = new UserGroupUser(userGroup2, user2);

            var userGroup = new UserGroup
            {
                Id = "User1",
                Name = "New Group1",
                UserGroupUsers = new UserGroupUser[] { userGroupUser1, userGroupUser2 },
            };
            var superUser = new User { Id = "superUser", Name = "New Group1", UserGroupUsers = userGroup.UserGroupUsers };

            var outputUserGroupDto1 = new OutputUserGroupDto
            {
                Id = "Group1",
                Name = "New Group1",
            };

            var allOutputUserDto = new OutputUserGroupDto[] { outputUserGroupDto1 };

            userRepositoryMoq.Setup(s => s.GetById(It.IsAny<string>())).ReturnsAsync(superUser);
            mapperMoq.Setup(s => s.Map<UserGroup[], OutputUserGroupDto[]>(It.IsAny<UserGroup[]>())).Returns(allOutputUserDto);

            var userClientServiceMoq = new UserClientService(userRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object,
                userValidatorMoq.Object, passwordValidatorMoq.Object, busMoq.Object, settingsMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>(), Mock.Of<IArcUserXmlWriter>());

            var retornoUserGroups = userClientServiceMoq.GetUserGroups("superUser");

            Assert.NotNull(retornoUserGroups.Result);
            Assert.Equal("Group1", retornoUserGroups.Result.Select(s => s.Id).FirstOrDefault());
        }

        [Fact]
        public void DissociateFromUserGroup()
        {
            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();

            var user = new User { Id = "User1", Name = "New User1", UserName = "new.user1", Email = "new.user1@email.com" };
            authorizationServiceMoq.Setup(s => s.DissociateUserFromUserGroup(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(user));

            var settingsAuthenticationType = SettingsAuthenticationType.User;
            settingsMoq.Setup(s => s.AuthenticationType).Returns(settingsAuthenticationType);

            var outputUserDto = new OutputUserDto
            {
                Id = "USR_GUID1",
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient1",
                Login = "login1",
                PasswordExpiration = DateTime.Now
            };

            mapperMoq.Setup(s => s.Map<User, OutputUserDto>(It.IsAny<User>())).Returns(outputUserDto);

            var userCreatedOrUpdatedEvent = new UserCreatedOrUpdatedEvent
            {
                User = outputUserDto,
                AuthenticationType = AuthenticationType.ActiveDirectory,
                PasswordHash = user.PasswordHash,
                HashArc = "",
                RequestUserId = ""
            };

            busMoq.Setup(s => s.Publish(It.IsAny<UserCreatedOrUpdatedEvent>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(userCreatedOrUpdatedEvent));

            var userClientServiceMoq = new UserClientService(userRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object,
                userValidatorMoq.Object, passwordValidatorMoq.Object, busMoq.Object, settingsMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>(), Mock.Of<IArcUserXmlWriter>());

            var retorno = userClientServiceMoq.DissociateFromUserGroup(user.Id, null, null);

            busMoq.Verify(v => v.Publish(It.IsAny<UserCreatedOrUpdatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            Assert.True(retorno.IsCompleted);
        }

        [Fact]
        public void AssociateToUserGroup()
        {
            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();

            var user = new User { Id = "User1", Name = "New User1", UserName = "new.user1", Email = "new.user1@email.com" };
            authorizationServiceMoq.Setup(s => s.AssociateUserToUserGroup(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(user));

            var settingsAuthenticationType = SettingsAuthenticationType.User;
            settingsMoq.Setup(s => s.AuthenticationType).Returns(settingsAuthenticationType);

            var outputUserDto = new OutputUserDto
            {
                Id = "USR_GUID1",
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient1",
                Login = "login1",
                PasswordExpiration = DateTime.Now
            };

            mapperMoq.Setup(s => s.Map<User, OutputUserDto>(It.IsAny<User>())).Returns(outputUserDto);

            var userCreatedOrUpdatedEvent = new UserCreatedOrUpdatedEvent
            {
                User = outputUserDto,
                AuthenticationType = AuthenticationType.ActiveDirectory,
                PasswordHash = user.PasswordHash,
                HashArc = "",
                RequestUserId = ""
            };

            busMoq.Setup(s => s.Publish(It.IsAny<UserCreatedOrUpdatedEvent>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(userCreatedOrUpdatedEvent));

            var userClientServiceMoq = new UserClientService(userRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object,
                userValidatorMoq.Object, passwordValidatorMoq.Object, busMoq.Object, settingsMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>(), Mock.Of<IArcUserXmlWriter>());

            var retorno = userClientServiceMoq.AssociateToUserGroup(user.Id, null, null);

            busMoq.Verify(v => v.Publish(It.IsAny<UserCreatedOrUpdatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            Assert.True(retorno.IsCompleted);
        }

        [Fact]
        public async Task UpdateUserClientService()
        {
            var inputUserDto = new InputUserDto
            {
                Name = "UserClient",
                Login = "login",
                PasswordExpiration = DateTime.Now,
                ArcXml = "",
                Password = "password",
                AuthenticationType = AuthenticationType.DatabaseUser
            };

            var outputUserDto = new OutputUserDto
            {
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient",
                Login = "login",
                PasswordExpiration = DateTime.Now

            };
            var user1 = new User { Id = "User1", Name = "New User1", UserName = "new.user1", Email = "new.user1@email.com" };
            var userGroup1 = new UserGroup { Id = "Group1", Name = "New Group1" };
            var userGroupUser1 = new UserGroupUser(userGroup1, user1);

            var user2 = new User { Id = "User1", Name = "New User1", UserName = "new.user2", Email = "new.user2@email.com" };
            var userGroup2 = new UserGroup { Id = "Group1", Name = "New Group1" };
            var userGroupUser2 = new UserGroupUser(userGroup2, user2);

            var userGroup = new UserGroup
            {
                Id = "User1",
                Name = "New Group1",
                UserGroupUsers = new UserGroupUser[] { userGroupUser1, userGroupUser2 },
            };

            var substituteUser = new User { Id = "sub" };
            var activeUser = new User { Id = "activeUser" };
            var userSubstitution = new UserSubstitution(activeUser, substituteUser);
            var UserSubstitutionsList = new List<UserSubstitution>();
            UserSubstitutionsList.Add(userSubstitution);

            var user = new User { Id = "superUser", Name = "New Group1", UserGroupUsers = userGroup.UserGroupUsers, UserSubstitutions = UserSubstitutionsList };

            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();

            mapperMoq.Setup(s => s.Map<User, OutputUserDto>(It.IsAny<User>())).Returns(outputUserDto);
            mapperMoq.Setup(s => s.Map(It.IsAny<InputUserDto>(), It.IsAny<User>())).Returns(user);

            ValidationResult validationResult = new ValidationResult { };
            passwordValidatorMoq.Setup(s => s.Validate(It.IsAny<string>())).Returns(validationResult);

            userRepositoryMoq.Setup(s => s.GetById(It.IsAny<string>())).Returns(Task.FromResult(user));

            SetMockConfigureParametersToCreateUpdate(userRepositoryMoq);

            userRepositoryMoq.Setup(s => s.Update(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(user);

            userRepositoryMoq.Setup(s => s.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(true);

            var userClientServiceMoq = new UserClientService(userRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object,
                userValidatorMoq.Object, passwordValidatorMoq.Object, busMoq.Object, settingsMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>(), Mock.Of<IArcUserXmlWriter>());

            var savedUser = await userClientServiceMoq.Update(user.Id, inputUserDto, null);

            Assert.NotNull(savedUser);
            Assert.Equal(savedUser.Name, inputUserDto.Name);
        }

        [Fact]
        public async Task UpdateUserClientServiceParametersValidation()
        {
            var inputUserDto = new InputUserDto
            {
                Name = "UserClient",
                PasswordExpiration = DateTime.Now,
                ArcXml = "",
                AuthenticationType = AuthenticationType.DatabaseUser
            };

            var outputUserDto = new OutputUserDto
            {
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient",
                Login = "login",
                PasswordExpiration = DateTime.Now

            };
            var user1 = new User { Id = "User1", Name = "New User1", UserName = "new.user1", Email = "new.user1@email.com" };
            var userGroup1 = new UserGroup { Id = "Group1", Name = "New Group1" };
            var userGroupUser1 = new UserGroupUser(userGroup1, user1);

            var user2 = new User { Id = "User1", Name = "New User1", UserName = "new.user2", Email = "new.user2@email.com" };
            var userGroup2 = new UserGroup { Id = "Group1", Name = "New Group1" };
            var userGroupUser2 = new UserGroupUser(userGroup2, user2);

            var userGroup = new UserGroup
            {
                Id = "User1",
                Name = "New Group1",
                UserGroupUsers = new UserGroupUser[] { userGroupUser1, userGroupUser2 },
            };

            var substituteUser = new User { Id = "sub" };
            var activeUser = new User { Id = "activeUser" };
            var userSubstitution = new UserSubstitution(activeUser, substituteUser);
            var UserSubstitutionsList = new List<UserSubstitution>();
            UserSubstitutionsList.Add(userSubstitution);

            var user = new User { Id = "superUser", Name = "New Group1", UserGroupUsers = userGroup.UserGroupUsers, UserSubstitutions = UserSubstitutionsList };

            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();

            mapperMoq.Setup(s => s.Map<User, OutputUserDto>(It.IsAny<User>())).Returns(outputUserDto);
            mapperMoq.Setup(s => s.Map(It.IsAny<InputUserDto>(), It.IsAny<User>())).Returns(user);

            ValidationResult validationResult = new ValidationResult { };
            passwordValidatorMoq.Setup(s => s.Validate(It.IsAny<string>())).Returns(validationResult);

            userRepositoryMoq.Setup(s => s.GetById(It.IsAny<string>())).Returns(Task.FromResult(user));

            SetMockConfigureParametersToCreateUpdate(userRepositoryMoq);

            userRepositoryMoq.Setup(s => s.Update(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(user);

            userRepositoryMoq.Setup(s => s.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(true);

            var userClientServiceMoq = new UserClientService(userRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object,
                userValidatorMoq.Object, passwordValidatorMoq.Object, busMoq.Object, settingsMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>(), Mock.Of<IArcUserXmlWriter>());

            var savedUser = await userClientServiceMoq.Update(user.Id, inputUserDto, null);

            Assert.NotNull(savedUser);
            Assert.Null(inputUserDto.Login);
            Assert.Null(inputUserDto.Password);
            Assert.Equal(savedUser.Name, inputUserDto.Name);
        }

        private static void SetMockConfigureParametersToCreateUpdate(Mock<IUserRepository> userRepositoryMoq, List<SqlParameter> sqlParameters = null)
        {
            sqlParameters ??= new List<SqlParameter>
            {
                new SqlParameter(Constants.cst_Erro, ""),
                new SqlParameter(Constants.cst_FoiAlterado, true)
            };

            userRepositoryMoq
                .Setup(s => s.ConfigureParametersToCreateUpdate(
                    out It.Ref<List<SqlParameter>>.IsAny, It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                .Callback((out List<SqlParameter> parameters, string param1, DateTime param2, string param3, string param4, int param5, string param6) =>
                {
                    parameters = sqlParameters;
                });
        }

        delegate void ConfigureParametersToCreateUpdateCallback(out List<SqlParameter> parameters, string param1, DateTime param2, string param3, string param4, int param5, string param6);

        [Fact]
        public async Task CreateApiUserClientService()
        {
            var inputUserDto = new InputUserDto
            {
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient",
                Login = "login",
                PasswordExpiration = DateTime.Now,
                ArcXml = ""
            };

            var outputUserDto = new OutputUserDto
            {
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient",
                Login = "login",
                PasswordExpiration = DateTime.Now

            };

            var user = new User
            {
                Name = "User"
            };

            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();

            mapperMoq.Setup(s => s.Map<InputUserDto, User>(It.IsAny<InputUserDto>())).Returns(user);
            mapperMoq.Setup(s => s.Map<User, OutputUserDto>(It.IsAny<User>())).Returns(outputUserDto);

            SetMockConfigureParametersToCreateUpdate(userRepositoryMoq);

            userRepositoryMoq.Setup(s => s.Create(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(user);

            userRepositoryMoq.Setup(s => s.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(true);

            var userClientServiceMoq = new UserClientService(userRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object,
                userValidatorMoq.Object, passwordValidatorMoq.Object, busMoq.Object, settingsMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>(), Mock.Of<IArcUserXmlWriter>());

            var createdUser = await userClientServiceMoq.CreateApi(inputUserDto, "", null);

            Assert.Equal(createdUser.Name, inputUserDto.Name);
            Assert.NotNull(createdUser);
        }

        [Fact]
        public async Task DeleteApiUserClient()
        {
            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();

            userRepositoryMoq.Setup(s => s.ConfigureParametersToRemove(out It.Ref<List<SqlParameter>>.IsAny, It.IsAny<string>()));

            userRepositoryMoq.Setup(s => s.Remove(It.IsAny<string>())).ReturnsAsync(true);

            userRepositoryMoq.Setup(s => s.RemoveItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(true);

            var userClientServiceMoq = new UserClientService(userRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object,
                userValidatorMoq.Object, passwordValidatorMoq.Object, busMoq.Object, settingsMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>(), Mock.Of<IArcUserXmlWriter>());

            await userClientServiceMoq.DeleteApi("", null);

            userRepositoryMoq.Verify(v => v.Remove(It.IsAny<string>()), Times.Once());
            userRepositoryMoq.Verify(v => v.ConfigureParametersToRemove(out It.Ref<List<SqlParameter>>.IsAny, It.IsAny<string>()), Times.Never());
            userRepositoryMoq.Verify(v => v.RemoveItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>()), Times.Never());
        }

        [Fact]
        public async Task UpdateApiUserClientService()
        {
            var inputUserDto = new InputUserDto
            {
                Name = "UserClient",
                Login = "login",
                PasswordExpiration = DateTime.Now,
                ArcXml = "",
                Password = "password",
                AuthenticationType = AuthenticationType.DatabaseUser
            };

            var outputUserDto = new OutputUserDto
            {
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient",
                Login = "login",
                PasswordExpiration = DateTime.Now

            };
            var user1 = new User { Id = "User1", Name = "New User1", UserName = "new.user1", Email = "new.user1@email.com" };
            var userGroup1 = new UserGroup { Id = "Group1", Name = "New Group1" };
            var userGroupUser1 = new UserGroupUser(userGroup1, user1);

            var user2 = new User { Id = "User1", Name = "New User1", UserName = "new.user2", Email = "new.user2@email.com" };
            var userGroup2 = new UserGroup { Id = "Group1", Name = "New Group1" };
            var userGroupUser2 = new UserGroupUser(userGroup2, user2);

            var userGroup = new UserGroup
            {
                Id = "User1",
                Name = "New Group1",
                UserGroupUsers = new UserGroupUser[] { userGroupUser1, userGroupUser2 },
            };

            var substituteUser = new User { Id = "sub" };
            var activeUser = new User { Id = "activeUser" };
            var userSubstitution = new UserSubstitution(activeUser, substituteUser);
            var UserSubstitutionsList = new List<UserSubstitution>();
            UserSubstitutionsList.Add(userSubstitution);

            var user = new User { Id = "superUser", Name = "New Group1", UserGroupUsers = userGroup.UserGroupUsers, UserSubstitutions = UserSubstitutionsList };

            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();

            mapperMoq.Setup(s => s.Map<User, OutputUserDto>(It.IsAny<User>())).Returns(outputUserDto);
            mapperMoq.Setup(s => s.Map(It.IsAny<InputUserDto>(), It.IsAny<User>())).Returns(user);

            ValidationResult validationResult = new ValidationResult { };
            passwordValidatorMoq.Setup(s => s.Validate(It.IsAny<string>())).Returns(validationResult);

            userRepositoryMoq.Setup(s => s.GetById(It.IsAny<string>())).Returns(Task.FromResult(user));

            SetMockConfigureParametersToCreateUpdate(userRepositoryMoq);

            userRepositoryMoq.Setup(s => s.Update(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(user);

            userRepositoryMoq.Setup(s => s.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(true);

            var userClientServiceMoq = new UserClientService(userRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object,
                userValidatorMoq.Object, passwordValidatorMoq.Object, busMoq.Object, settingsMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>(), Mock.Of<IArcUserXmlWriter>());

            var savedUser = await userClientServiceMoq.UpdateApi(user.Id, inputUserDto, null);

            Assert.NotNull(savedUser);
            Assert.Equal(savedUser.Name, inputUserDto.Name);
        }

        [Fact]
        public async Task CreateApiUserClientServiceParametersValidation()
        {
            var inputUserDto = new InputUserDto
            {
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient",
                Login = "login",
                PasswordExpiration = DateTime.Now,
                Active = true,
                AuthenticationType = null,
                Password = "teste",
                ArcXml = ""
            };

            var outputUserDto = new OutputUserDto
            {
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient",
                Login = "login",
                PasswordExpiration = DateTime.Now

            };

            var user = new User
            {
                Name = "User"
            };

            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();

            mapperMoq.Setup(s => s.Map<InputUserDto, User>(It.IsAny<InputUserDto>())).Returns(user);
            mapperMoq.Setup(s => s.Map<User, OutputUserDto>(It.IsAny<User>())).Returns(outputUserDto);
            settingsMoq.Setup(s => s.AuthenticationType).Returns(SettingsAuthenticationType.User);

            SetMockConfigureParametersToCreateUpdate(userRepositoryMoq);

            userRepositoryMoq.Setup(s => s.Create(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(user);

            userRepositoryMoq.Setup(s => s.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(true);

            var userClientServiceMoq = new UserClientService(userRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object,
                userValidatorMoq.Object, passwordValidatorMoq.Object, busMoq.Object, settingsMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>(), Mock.Of<IArcUserXmlWriter>());

            var createdUser = await userClientServiceMoq.CreateApi(inputUserDto, "", null);

            Assert.Equal(createdUser.Name, inputUserDto.Name);
            Assert.NotNull(createdUser.AuthenticationType);
            Assert.NotNull(createdUser);
        }

        [Fact]
        public async Task UpdateApiUserClientServiceParametersValidation()
        {
            var inputUserDto = new InputUserDto
            {
                Name = "UserClient",
                PasswordExpiration = DateTime.Now,
                ArcXml = "",
                AuthenticationType = AuthenticationType.DatabaseUser
            };

            var outputUserDto = new OutputUserDto
            {
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient",
                Login = "login",
                PasswordExpiration = DateTime.Now

            };
            var user1 = new User { Id = "User1", Name = "New User1", UserName = "new.user1", Email = "new.user1@email.com" };
            var userGroup1 = new UserGroup { Id = "Group1", Name = "New Group1" };
            var userGroupUser1 = new UserGroupUser(userGroup1, user1);

            var user2 = new User { Id = "User1", Name = "New User1", UserName = "new.user2", Email = "new.user2@email.com" };
            var userGroup2 = new UserGroup { Id = "Group1", Name = "New Group1" };
            var userGroupUser2 = new UserGroupUser(userGroup2, user2);

            var userGroup = new UserGroup
            {
                Id = "User1",
                Name = "New Group1",
                UserGroupUsers = new UserGroupUser[] { userGroupUser1, userGroupUser2 },
            };

            var substituteUser = new User { Id = "sub" };
            var activeUser = new User { Id = "activeUser" };
            var userSubstitution = new UserSubstitution(activeUser, substituteUser);
            var UserSubstitutionsList = new List<UserSubstitution>
            {
                userSubstitution
            };

            var user = new User { Id = "superUser", Name = "New Group1", UserGroupUsers = userGroup.UserGroupUsers, UserSubstitutions = UserSubstitutionsList };

            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();

            mapperMoq.Setup(s => s.Map<User, OutputUserDto>(It.IsAny<User>())).Returns(outputUserDto);
            mapperMoq.Setup(s => s.Map(It.IsAny<InputUserDto>(), It.IsAny<User>())).Returns(user);

            ValidationResult validationResult = new ValidationResult { };
            passwordValidatorMoq.Setup(s => s.Validate(It.IsAny<string>())).Returns(validationResult);

            userRepositoryMoq.Setup(s => s.GetById(It.IsAny<string>())).Returns(Task.FromResult(user));

            SetMockConfigureParametersToCreateUpdate(userRepositoryMoq);

            userRepositoryMoq.Setup(s => s.Update(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(user);

            userRepositoryMoq.Setup(s => s.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(true);

            var userClientServiceMoq = new UserClientService(userRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object,
                userValidatorMoq.Object, passwordValidatorMoq.Object, busMoq.Object, settingsMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>(), Mock.Of<IArcUserXmlWriter>());

            var savedUser = await userClientServiceMoq.UpdateApi(user.Id, inputUserDto, null);

            Assert.NotNull(savedUser);
            Assert.Equal(savedUser.Name, inputUserDto.Name);
        }

        [Fact]
        public void GetById_ReturnsUserWithPasswordForDatabaseAuthentication()
        {
            var userId = "testUserId";
            var expectedUser = new User
            {
                Id = userId,
                PasswordHash = "password",
                AuthenticationType = AuthenticationType.DatabaseUser.ToString()
            };
            var expectedOutputUserDto = new OutputUserDto
            {
                Login = "testUser",
                AuthenticationType = AuthenticationType.DatabaseUser
            };

            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();

            var userClientServiceMoq = new UserClientService(userRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object,
                userValidatorMoq.Object, passwordValidatorMoq.Object, busMoq.Object, settingsMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>(), Mock.Of<IArcUserXmlWriter>());

            userRepositoryMoq.Setup(repo => repo.GetById(userId)).ReturnsAsync(expectedUser);
            mapperMoq.Setup(mapper => mapper.Map<User, OutputUserDto>(It.IsAny<User>())).Returns(expectedOutputUserDto);

            var result = userClientServiceMoq.GetById(userId, out string password);

            Assert.Equal("password", password);
            Assert.Equal(AuthenticationType.DatabaseUser, result.AuthenticationType);
            userRepositoryMoq.Verify(repo => repo.GetById(userId), Times.Once);
            mapperMoq.Verify(mapper => mapper.Map<User, OutputUserDto>(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public void GetById_ReturnsUserWithNoPasswordForActiveDirectoryAuthentication()
        {
            var userId = "testUserId";
            var expectedUser = new User
            {
                Id = userId,
                PasswordHash = "password",
                AuthenticationType = AuthenticationType.ActiveDirectory.ToString()
            };
            var expectedOutputUserDto = new OutputUserDto
            {
                Login = "testUser",
                AuthenticationType = AuthenticationType.ActiveDirectory
            };

            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();

            var userClientServiceMoq = new UserClientService(userRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object,
                userValidatorMoq.Object, passwordValidatorMoq.Object, busMoq.Object, settingsMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>(), Mock.Of<IArcUserXmlWriter>());

            userRepositoryMoq.Setup(repo => repo.GetById(userId)).ReturnsAsync(expectedUser);
            mapperMoq.Setup(mapper => mapper.Map<User, OutputUserDto>(It.IsAny<User>())).Returns(expectedOutputUserDto);

            var result = userClientServiceMoq.GetById(userId, out string password);

            Assert.Equal(string.Empty, password);
            Assert.Equal(AuthenticationType.ActiveDirectory, result.AuthenticationType);
            userRepositoryMoq.Verify(repo => repo.GetById(userId), Times.Once);
            mapperMoq.Verify(mapper => mapper.Map<User, OutputUserDto>(It.IsAny<User>()), Times.Once);
        }
    }
}
