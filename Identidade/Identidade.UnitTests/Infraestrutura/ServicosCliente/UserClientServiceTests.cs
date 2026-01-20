using Identidade.Dominio.Fabricas;
using Identidade.Dominio.Helpers;
using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Modelos;
using Identidade.Dominio.Repositorios;
using Identidade.Dominio.Servicos;
using Identidade.Infraestrutura.Configuracoes;
using Identidade.Infraestrutura.Helpers;
using Identidade.Infraestrutura.ServicosCliente;
using Identidade.Publico.Dtos;
using Identidade.Publico.Enumerations;
using Identidade.Publico.Events;
using Identidade.UnitTests.Helpers;
using MassTransit;
using Microsoft.Data.SqlClient;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace Identidade.UnitTests.Infraestrutura.ServicosCliente
{
    public class UserClientServiceTests
    {
        public static IEnumerable<object[]> GetUserClientServiceConstructorTestParameters()
        {
            return ParameterTestHelper.GetParameters(s => s
                .AddNullableParameter("userRepository", Mock.Of<IUserRepository>())
                .AddNullableParameter("authorizationService", Mock.Of<IAuthorizationService>())
                .AddNullableParameter("userValidator", Mock.Of<IUserValidator>())
                .AddNullableParameter("passwordValidator", Mock.Of<IPasswordValidator>())
                .AddNullableParameter("bus", Mock.Of<IBus>())
                .AddNullableParameter("settings", Mock.Of<ISettings>())
                .AddNullableParameter("databaseConnectionModifier", Mock.Of<IDatabaseConnectionUserModifier>())
                .AddNullableParameter("arcUserXmlWriter", Mock.Of<IArcUserXmlWriter>())
                .AddNullableParameter("fabricaUsuario", Mock.Of<IFabricaUsuario>()));
        }

        [Theory]
        [MemberData(nameof(GetUserClientServiceConstructorTestParameters))]
        public void UserClientServiceConstructorTest(IUserRepository userRepository, IAuthorizationService authorizationService, 
            IUserValidator userValidator, IPasswordValidator passwordValidator, IBus bus, ISettings settings, IDatabaseConnectionUserModifier databaseConnectionModifier, IArcUserXmlWriter arcUserXmlWriter, IFabricaUsuario fabricaUsuario,
            string missingParameterName = null)
        {
            UserClientService Create() => new UserClientService(userRepository, authorizationService, 
                userValidator, passwordValidator, bus, settings, databaseConnectionModifier, arcUserXmlWriter, fabricaUsuario);

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

            var mappedUser = new User
            {
                Name = inputUserDto.Name,
                UserName = inputUserDto.Login
            };

            var outputUserDto = new OutputUserDto
            {
                Name = inputUserDto.Name,
                Login = inputUserDto.Login,
                PasswordExpiration = inputUserDto.PasswordExpiration
            };

            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var fabricaUsuarioMoq = new Mock<IFabricaUsuario>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();

            fabricaUsuarioMoq
                .Setup(s => s.MapearParaUsuarioAsync(It.IsAny<InputUserDto>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mappedUser);

            fabricaUsuarioMoq
                .Setup(s => s.MapearParaDtoSaidaUsuario(It.IsAny<User>()))
                .Returns(outputUserDto);

            SetMockConfigureParametersToCreateUpdate(userRepositoryMoq);

            userRepositoryMoq.Setup(s => s.Create(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(mappedUser);
            userRepositoryMoq.Setup(s => s.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(true);

            var userClientServiceMoq = new UserClientService(
                userRepositoryMoq.Object,
                authorizationServiceMoq.Object,
                userValidatorMoq.Object,
                passwordValidatorMoq.Object,
                busMoq.Object,
                settingsMoq.Object,
                Mock.Of<IDatabaseConnectionUserModifier>(),
                Mock.Of<IArcUserXmlWriter>(),
                fabricaUsuarioMoq.Object);

            var createdUser = await userClientServiceMoq.Create(inputUserDto, "", null);

            Assert.NotNull(createdUser);
            Assert.Equal(inputUserDto.Name, createdUser.Name);
            Assert.Equal(inputUserDto.Login, createdUser.Login);
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

            var mappedUser = new User
            {
                Name = inputUserDto.Name,
                UserName = inputUserDto.Login
            };

            var outputUserDto = new OutputUserDto
            {
                Name = inputUserDto.Name,
                Login = inputUserDto.Login,
                PasswordExpiration = inputUserDto.PasswordExpiration,
                // FabricaUsuario returns empty arrays when null
                UserGroups = Array.Empty<string>(),
                SubstituteUsers = Array.Empty<string>()
            };

            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var fabricaUsuarioMoq = new Mock<IFabricaUsuario>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();

            fabricaUsuarioMoq
                .Setup(s => s.MapearParaUsuarioAsync(It.IsAny<InputUserDto>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mappedUser);

            fabricaUsuarioMoq
                .Setup(s => s.MapearParaDtoSaidaUsuario(It.IsAny<User>()))
                .Returns(outputUserDto);

            SetMockConfigureParametersToCreateUpdate(userRepositoryMoq);

            userRepositoryMoq.Setup(s => s.Create(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(mappedUser);
            userRepositoryMoq.Setup(s => s.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(true);

            var userClientServiceMoq = new UserClientService(
                userRepositoryMoq.Object,
                authorizationServiceMoq.Object,
                userValidatorMoq.Object,
                passwordValidatorMoq.Object,
                busMoq.Object,
                settingsMoq.Object,
                Mock.Of<IDatabaseConnectionUserModifier>(),
                Mock.Of<IArcUserXmlWriter>(),
                fabricaUsuarioMoq.Object);

            var createdUser = await userClientServiceMoq.Create(inputUserDto, "", null);

            Assert.NotNull(createdUser);
            Assert.Equal(inputUserDto.Name, createdUser.Name);
            userValidatorMoq.Verify(p => p.VerifyExistences(null, null), Times.Once);
            Assert.NotNull(createdUser.UserGroups);
            Assert.NotNull(createdUser.SubstituteUsers);
            Assert.Empty(createdUser.UserGroups);
            Assert.Empty(createdUser.SubstituteUsers);
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

            var mappedUser = new User
            {
                Name = inputUserDto.Name,
                UserName = inputUserDto.Login
            };

            var outputUserDto = new OutputUserDto
            {
                Name = inputUserDto.Name,
                Login = inputUserDto.Login,
                PasswordExpiration = inputUserDto.PasswordExpiration,
                AuthenticationType = null
            };

            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var fabricaUsuarioMoq = new Mock<IFabricaUsuario>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();

            fabricaUsuarioMoq
                .Setup(s => s.MapearParaUsuarioAsync(It.IsAny<InputUserDto>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mappedUser);
            fabricaUsuarioMoq
                .Setup(s => s.MapearParaDtoSaidaUsuario(It.IsAny<User>()))
                .Returns(outputUserDto);

            settingsMoq.Setup(s => s.AuthenticationType).Returns(SettingsAuthenticationType.User);

            SetMockConfigureParametersToCreateUpdate(userRepositoryMoq);

            userRepositoryMoq.Setup(s => s.Create(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(mappedUser);
            userRepositoryMoq.Setup(s => s.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(true);

            var userClientServiceMoq = new UserClientService(
                userRepositoryMoq.Object,
                authorizationServiceMoq.Object,
                userValidatorMoq.Object,
                passwordValidatorMoq.Object,
                busMoq.Object,
                settingsMoq.Object,
                Mock.Of<IDatabaseConnectionUserModifier>(),
                Mock.Of<IArcUserXmlWriter>(),
                fabricaUsuarioMoq.Object);

            var createdUser = await userClientServiceMoq.Create(inputUserDto, "", null);

            Assert.NotNull(createdUser);
            Assert.Equal(inputUserDto.Name, createdUser.Name);
            Assert.Null(createdUser.AuthenticationType);
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

            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();
            var fabricaUsuarioMoq = new Mock<IFabricaUsuario>();

            settingsMoq.Setup(s => s.AuthenticationType).Returns(SettingsAuthenticationType.User);

            var userClientServiceMoq = new UserClientService(
                userRepositoryMoq.Object,
                authorizationServiceMoq.Object,
                Mock.Of<IUserValidator>(),
                passwordValidatorMoq.Object,
                busMoq.Object,
                settingsMoq.Object,
                Mock.Of<IDatabaseConnectionUserModifier>(),
                Mock.Of<IArcUserXmlWriter>(),
                fabricaUsuarioMoq.Object);

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
            var fabricaUsuarioMoq = new Mock<IFabricaUsuario>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();
            var arcUserXmlWriteMoq = new Mock<IArcUserXmlWriter>();

            fabricaUsuarioMoq.Setup(s => s.MapearParaUsuarioAsync(It.IsAny<ArcUserDto>(), null, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            fabricaUsuarioMoq.Setup(s => s.MapearParaArcUser(It.IsAny<ArcUserDto>())).Returns(arcUser);
            fabricaUsuarioMoq.Setup(s => s.MapearParaDtoSaidaUsuario(It.IsAny<User>())).Returns(outputUserDto);

            settingsMoq.Setup(s => s.AuthenticationType).Returns(SettingsAuthenticationType.User);

            SetMockConfigureParametersToCreateUpdate(userRepositoryMoq);

            userRepositoryMoq.Setup(s => s.Create(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(user);
            userRepositoryMoq.Setup(s => s.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(true);

            arcUserXmlWriteMoq.Setup(s => s.Write(arcUser, It.IsAny<string>(), user.PasswordHistory)).Returns(new XElement("root"));

            var userClientServiceMoq = new UserClientService(
                userRepositoryMoq.Object,
                authorizationServiceMoq.Object,
                userValidatorMoq.Object,
                passwordValidatorMoq.Object,
                busMoq.Object,
                settingsMoq.Object,
                Mock.Of<IDatabaseConnectionUserModifier>(),
                arcUserXmlWriteMoq.Object,
                fabricaUsuarioMoq.Object);

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
            var fabricaUsuarioMoq = new Mock<IFabricaUsuario>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();

            fabricaUsuarioMoq.Setup(s => s.MapearParaUsuarioAsync(It.IsAny<ArcUserDto>(), null, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            fabricaUsuarioMoq.Setup(s => s.MapearParaDtoSaidaUsuario(It.IsAny<User>())).Returns(outputUserDto);

            settingsMoq.Setup(s => s.AuthenticationType).Returns(SettingsAuthenticationType.User);

            userRepositoryMoq.Setup(s => s.Create(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync((User)null);

            var userClientServiceMoq = new UserClientService(
                userRepositoryMoq.Object,
                authorizationServiceMoq.Object,
                userValidatorMoq.Object,
                passwordValidatorMoq.Object,
                busMoq.Object,
                settingsMoq.Object,
                Mock.Of<IDatabaseConnectionUserModifier>(),
                Mock.Of<IArcUserXmlWriter>(),
                fabricaUsuarioMoq.Object);

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
            var fabricaUsuarioMoq = new Mock<IFabricaUsuario>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();
            var arcUserXmlWriteMoq = new Mock<IArcUserXmlWriter>();

            fabricaUsuarioMoq.Setup(s => s.MapearParaUsuarioAsync(It.IsAny<ArcUserDto>(), null, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            fabricaUsuarioMoq.Setup(s => s.MapearParaDtoSaidaUsuario(It.IsAny<User>())).Returns(outputUserDto);

            settingsMoq.Setup(s => s.AuthenticationType).Returns(SettingsAuthenticationType.User);

            var sqlParameters = new List<SqlParameter>
            {
                new SqlParameter(Constants.cst_Erro, expectedError)
            };
            SetMockConfigureParametersToCreateUpdate(userRepositoryMoq, sqlParameters);

            userRepositoryMoq.Setup(s => s.Create(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(user);

            userRepositoryMoq.Setup(s => s.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(false);

            arcUserXmlWriteMoq.Setup(s => s.Write(arcUser, It.IsAny<string>(), user.PasswordHistory)).Returns(new XElement("root"));

            var userClientServiceMoq = new UserClientService(
                userRepositoryMoq.Object,
                authorizationServiceMoq.Object,
                userValidatorMoq.Object,
                passwordValidatorMoq.Object,
                busMoq.Object,
                settingsMoq.Object,
                Mock.Of<IDatabaseConnectionUserModifier>(),
                arcUserXmlWriteMoq.Object,
                fabricaUsuarioMoq.Object);

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
            var fabricaUsuarioMoq = new Mock<IFabricaUsuario>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();
            var arcUserXmlWriteMoq = new Mock<IArcUserXmlWriter>();

            fabricaUsuarioMoq.Setup(s => s.MapearParaUsuarioAsync(It.IsAny<ArcUserDto>(), null, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            fabricaUsuarioMoq.Setup(s => s.MapearParaDtoSaidaUsuario(It.IsAny<User>())).Returns(outputUserDto);

            settingsMoq.Setup(s => s.AuthenticationType).Returns(SettingsAuthenticationType.User);

            var sqlParameters = new List<SqlParameter>
            {
                new SqlParameter(Constants.cst_FoiAlterado, false)
            };
            SetMockConfigureParametersToCreateUpdate(userRepositoryMoq, sqlParameters);

            userRepositoryMoq.Setup(s => s.Create(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(user);

            userRepositoryMoq.Setup(s => s.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(false);

            arcUserXmlWriteMoq.Setup(s => s.Write(arcUser, It.IsAny<string>(), user.PasswordHistory)).Returns(new XElement("root"));

            var userClientServiceMoq = new UserClientService(
                userRepositoryMoq.Object,
                authorizationServiceMoq.Object,
                userValidatorMoq.Object,
                passwordValidatorMoq.Object,
                busMoq.Object,
                settingsMoq.Object,
                Mock.Of<IDatabaseConnectionUserModifier>(),
                arcUserXmlWriteMoq.Object,
                fabricaUsuarioMoq.Object);

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
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();
            var fabricaUsuarioMoq = new Mock<IFabricaUsuario>();

            fabricaUsuarioMoq
                .Setup(s => s.ValidarArcUserDto(It.IsAny<ArcUserDto>()))
                .Callback<ArcUserDto>(dto =>
                {
                    // reproduce real behavior for the validation scenarios in this test
                    new FabricaUsuario(Mock.Of<IReadOnlyRepository<User>>(), Mock.Of<IUserGroupRepository>()).ValidarArcUserDto(dto);
                });

            if (!validPassword)
                passwordValidatorMoq.Setup(s => s.Validate(It.IsAny<string>())).Throws(expectedException);

            var userClientServiceMoq = new UserClientService(
                userRepositoryMoq.Object,
                authorizationServiceMoq.Object,
                Mock.Of<IUserValidator>(),
                passwordValidatorMoq.Object,
                busMoq.Object,
                settingsMoq.Object,
                Mock.Of<IDatabaseConnectionUserModifier>(),
                Mock.Of<IArcUserXmlWriter>(),
                fabricaUsuarioMoq.Object);

            var ex = Assert.ThrowsAsync(expectedException.GetType(), async () => await userClientServiceMoq.Create(arcUser, string.Empty, null));
            Assert.Equal(expectedException.Message, ex.Result.Message);
        }

        [Fact]
        public async Task UpdateFromArcUserUserClientServiceTest()
        {
            var arcUserDto = new ArcUserDto(
                "TestEmail@test.com",
                "TestPassword",
                new DateTimeOffset(2023, 04, 18, 1, 2, 3, TimeSpan.FromHours(3)).ToString(),
                false,
                true,
                AuthenticationType.DatabaseUser,
                Language.Portugues)
            {
                SubstituteUsers = new string[] { "", "", "" },
                UserGroups = new string[] { "", "", "" },
                Name = "UserClient",
                Login = "login"
            };

            var arcUser = new ArcUser(
                "login",
                "UserClient",
                "TestEmail@test.com",
                "TestPassword",
                Language.Portugues.ToString(),
                null,
                null,
                AuthenticationType.DatabaseUser.ToString(),
                new DateTimeOffset(2023, 04, 18, 1, 2, 3, TimeSpan.FromHours(3)),
                false,
                true);

            var outputUserDto = new OutputUserDto
            {
                Name = "UserClient",
                Login = "login",
                PasswordExpiration = DateTime.Now
            };

            var currentUser = new User
            {
                Id = "id",
                UserName = "login",
                UserGroupUsers = new List<UserGroupUser>(),
                UserSubstitutions = new List<UserSubstitution>(),
                PasswordHistory = "testHistory"
            };

            var updatedUser = new User
            {
                Id = currentUser.Id,
                UserName = arcUserDto.Login,
                Name = arcUserDto.Name,
                PasswordHistory = currentUser.PasswordHistory
            };

            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var fabricaUsuarioMoq = new Mock<IFabricaUsuario>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();
            var arcUserXmlWriteMoq = new Mock<IArcUserXmlWriter>();

            fabricaUsuarioMoq.Setup(s => s.ValidarArcUserDto(It.IsAny<ArcUserDto>()));
            fabricaUsuarioMoq.Setup(s => s.MapearParaUsuarioAsync(It.IsAny<ArcUserDto>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(updatedUser);
            fabricaUsuarioMoq.Setup(s => s.MapearParaArcUser(It.IsAny<ArcUserDto>())).Returns(arcUser);
            fabricaUsuarioMoq.Setup(s => s.MapearParaDtoSaidaUsuario(It.IsAny<User>())).Returns(outputUserDto);

            settingsMoq.Setup(s => s.AuthenticationType).Returns(SettingsAuthenticationType.User);

            SetMockConfigureParametersToCreateUpdate(userRepositoryMoq);

            userRepositoryMoq.Setup(s => s.GetById(It.IsAny<string>())).ReturnsAsync(currentUser);
            userRepositoryMoq.Setup(s => s.Update(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(updatedUser);
            userRepositoryMoq.Setup(s => s.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(true);

            arcUserXmlWriteMoq.Setup(s => s.Write(arcUser, It.IsAny<string>(), updatedUser.PasswordHistory)).Returns(new XElement("root"));

            var userClientServiceMoq = new UserClientService(
                userRepositoryMoq.Object,
                authorizationServiceMoq.Object,
                userValidatorMoq.Object,
                passwordValidatorMoq.Object,
                busMoq.Object,
                settingsMoq.Object,
                Mock.Of<IDatabaseConnectionUserModifier>(),
                arcUserXmlWriteMoq.Object,
                fabricaUsuarioMoq.Object);

            var result = await userClientServiceMoq.Update(string.Empty, arcUserDto, null);

            Assert.NotNull(result);
            Assert.Equal(outputUserDto.Name, result.Name);
            Assert.Equal(outputUserDto.Login, result.Login);
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
            var fabricaUsuarioMoq = new Mock<IFabricaUsuario>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();

            fabricaUsuarioMoq.Setup(s => s.MapearParaUsuarioAsync(It.IsAny<ArcUserDto>(), null, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            fabricaUsuarioMoq.Setup(s => s.MapearParaDtoSaidaUsuario(It.IsAny<User>())).Returns(outputUserDto);

            settingsMoq.Setup(s => s.AuthenticationType).Returns(SettingsAuthenticationType.User);

            userRepositoryMoq.Setup(s => s.Update(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync((User)null);

            var userClientServiceMoq = new UserClientService(
                userRepositoryMoq.Object,
                authorizationServiceMoq.Object,
                userValidatorMoq.Object,
                passwordValidatorMoq.Object,
                busMoq.Object,
                settingsMoq.Object,
                Mock.Of<IDatabaseConnectionUserModifier>(),
                Mock.Of<IArcUserXmlWriter>(),
                fabricaUsuarioMoq.Object);

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
            var fabricaUsuarioMoq = new Mock<IFabricaUsuario>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();
            var arcUserXmlWriteMoq = new Mock<IArcUserXmlWriter>();

            fabricaUsuarioMoq.Setup(s => s.MapearParaUsuarioAsync(It.IsAny<ArcUserDto>(), null, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            fabricaUsuarioMoq.Setup(s => s.MapearParaDtoSaidaUsuario(It.IsAny<User>())).Returns(outputUserDto);

            settingsMoq.Setup(s => s.AuthenticationType).Returns(SettingsAuthenticationType.User);

            var sqlParameters = new List<SqlParameter>
            {
                new SqlParameter(Constants.cst_Erro, expectedError),
            };
            SetMockConfigureParametersToCreateUpdate(userRepositoryMoq, sqlParameters);

            userRepositoryMoq.Setup(s => s.Update(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(user);

            userRepositoryMoq.Setup(s => s.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(false);

            arcUserXmlWriteMoq.Setup(s => s.Write(arcUser, It.IsAny<string>(), user.PasswordHistory)).Returns(new XElement("root"));

            var userClientServiceMoq = new UserClientService(
                userRepositoryMoq.Object,
                authorizationServiceMoq.Object,
                userValidatorMoq.Object,
                passwordValidatorMoq.Object,
                busMoq.Object,
                settingsMoq.Object,
                Mock.Of<IDatabaseConnectionUserModifier>(),
                arcUserXmlWriteMoq.Object,
                fabricaUsuarioMoq.Object);

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
            var fabricaUsuarioMoq = new Mock<IFabricaUsuario>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();
            var arcUserXmlWriteMoq = new Mock<IArcUserXmlWriter>();

            fabricaUsuarioMoq.Setup(s => s.MapearParaUsuarioAsync(It.IsAny<ArcUserDto>(), null, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            fabricaUsuarioMoq.Setup(s => s.MapearParaDtoSaidaUsuario(It.IsAny<User>())).Returns(outputUserDto);

            settingsMoq.Setup(s => s.AuthenticationType).Returns(SettingsAuthenticationType.User);

            var sqlParameters = new List<SqlParameter>
            {
                new SqlParameter(Constants.cst_FoiAlterado, false),
            };
            SetMockConfigureParametersToCreateUpdate(userRepositoryMoq, sqlParameters);

            userRepositoryMoq.Setup(s => s.Update(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(user);

            userRepositoryMoq.Setup(s => s.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(false);

            arcUserXmlWriteMoq.Setup(s => s.Write(arcUser, It.IsAny<string>(), user.PasswordHistory)).Returns(new XElement("root"));

            var userClientServiceMoq = new UserClientService(
                userRepositoryMoq.Object,
                authorizationServiceMoq.Object,
                userValidatorMoq.Object,
                passwordValidatorMoq.Object,
                busMoq.Object,
                settingsMoq.Object,
                Mock.Of<IDatabaseConnectionUserModifier>(),
                arcUserXmlWriteMoq.Object,
                fabricaUsuarioMoq.Object);

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await userClientServiceMoq.Update(string.Empty, arcUserDto, null));
            Assert.Equal(Constants.Exception.cst_UserUpdateFailedNoChange, ex.Result.Message);
            return Task.CompletedTask;
        }

        [Fact]
        public async Task DeleteUserClient()
        {
            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();
            var fabricaUsuarioMoq = new Mock<IFabricaUsuario>();

            userRepositoryMoq.Setup(s => s.ConfigureParametersToRemove(out It.Ref<List<SqlParameter>>.IsAny, It.IsAny<string>()));

            userRepositoryMoq.Setup(s => s.Remove(It.IsAny<string>())).ReturnsAsync(true);

            userRepositoryMoq.Setup(s => s.RemoveItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(true);

            var userClientServiceMoq = new UserClientService(
                userRepositoryMoq.Object,
                authorizationServiceMoq.Object,
                userValidatorMoq.Object,
                passwordValidatorMoq.Object,
                busMoq.Object,
                settingsMoq.Object,
                Mock.Of<IDatabaseConnectionUserModifier>(),
                Mock.Of<IArcUserXmlWriter>(),
                fabricaUsuarioMoq.Object);

            await userClientServiceMoq.Delete("", null);

            userRepositoryMoq.Verify(v => v.Remove(It.IsAny<string>()), Times.Once());
            userRepositoryMoq.Verify(v => v.ConfigureParametersToRemove(out It.Ref<List<SqlParameter>>.IsAny, It.IsAny<string>()), Times.Once());
            userRepositoryMoq.Verify(v => v.RemoveItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>()), Times.Once());
        }

        [Fact]
        public async Task GetIReadOnlyCollectionOutputUserDtoGetAll()
        {
            var user1 = new User { Id = "USR_GUID1", UserName = "user1" };
            var user2 = new User { Id = "USR_GUID2", UserName = "user2" };

            var outputUserDto1 = new OutputUserDto
            {
                Name = "UserClient1",
                Login = "login1",
                PasswordExpiration = DateTime.Now
            };

            var outputUserDto2 = new OutputUserDto
            {
                Name = "UserClient2",
                Login = "login2",
                PasswordExpiration = DateTime.Now
            };

            var allUsers = new List<User> { user1, user2 };
            var allUsersDto = new List<OutputUserDto> { outputUserDto1, outputUserDto2 };

            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();
            var fabricaUsuarioMoq = new Mock<IFabricaUsuario>();

            userRepositoryMoq.Setup(s => s.GetAll()).ReturnsAsync(allUsers);
            fabricaUsuarioMoq
                .Setup(s => s.MapearParaDtoSaidaUsuario(It.IsAny<User>()))
                .Returns((User u) => allUsersDto.First(d => d.Login == (u.UserName == "user1" ? "login1" : "login2")));

            var userClientServiceMoq = new UserClientService(
                userRepositoryMoq.Object,
                authorizationServiceMoq.Object,
                userValidatorMoq.Object,
                passwordValidatorMoq.Object,
                busMoq.Object,
                settingsMoq.Object,
                Mock.Of<IDatabaseConnectionUserModifier>(),
                Mock.Of<IArcUserXmlWriter>(),
                fabricaUsuarioMoq.Object);

            var retornoGetAll = await userClientServiceMoq.Get(null);

            Assert.NotNull(retornoGetAll);
            Assert.Equal(allUsers.Count, retornoGetAll.Count);
        }

        [Fact]
        public async Task GetIReadOnlyCollectionOutputUserDtoGetByLogin()
        {
            var user = new User { Id = "USR_GUID1", UserName = "UserClient1" };

            var outputUserDto = new OutputUserDto
            {
                Name = "UserClient1",
                Login = "login1",
                PasswordExpiration = DateTime.Now
            };

            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();
            var fabricaUsuarioMoq = new Mock<IFabricaUsuario>();

            userRepositoryMoq.Setup(s => s.GetByName(It.IsAny<string>())).ReturnsAsync(user);
            fabricaUsuarioMoq.Setup(s => s.MapearParaDtoSaidaUsuario(It.IsAny<User>())).Returns(outputUserDto);

            var userClientServiceMoq = new UserClientService(
                userRepositoryMoq.Object,
                authorizationServiceMoq.Object,
                userValidatorMoq.Object,
                passwordValidatorMoq.Object,
                busMoq.Object,
                settingsMoq.Object,
                Mock.Of<IDatabaseConnectionUserModifier>(),
                Mock.Of<IArcUserXmlWriter>(),
                fabricaUsuarioMoq.Object);

            var retornoGetByName = await userClientServiceMoq.Get("login1");

            Assert.NotNull(retornoGetByName);
            Assert.Equal("UserClient1", retornoGetByName.Select(s => s.Name).FirstOrDefault());
        }

        [Fact]
        public async Task GetById()
        {
            var user = new User { Id = "USR_GUID1", UserName = "UserClient1" };

            var outputUserDto = new OutputUserDto
            {
                Id = "USR_GUID1",
                Name = "UserClient1",
                Login = "login1",
                PasswordExpiration = DateTime.Now
            };

            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();
            var fabricaUsuarioMoq = new Mock<IFabricaUsuario>();

            userRepositoryMoq.Setup(s => s.GetById(It.IsAny<string>())).ReturnsAsync(user);
            fabricaUsuarioMoq.Setup(s => s.MapearParaDtoSaidaUsuario(It.IsAny<User>())).Returns(outputUserDto);

            var userClientServiceMoq = new UserClientService(
                userRepositoryMoq.Object,
                authorizationServiceMoq.Object,
                userValidatorMoq.Object,
                passwordValidatorMoq.Object,
                busMoq.Object,
                settingsMoq.Object,
                Mock.Of<IDatabaseConnectionUserModifier>(),
                Mock.Of<IArcUserXmlWriter>(),
                fabricaUsuarioMoq.Object);

            var retornoGetById = await userClientServiceMoq.GetById("USR_GUID1");

            Assert.NotNull(retornoGetById);
            Assert.Equal("USR_GUID1", retornoGetById.Id);
        }

        [Fact]
        public async Task GetUserGroups()
        {
            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();
            var fabricaUsuarioMoq = new Mock<IFabricaUsuario>();

            var user1 = new User { Id = "User1", Name = "New User1", UserName = "new.user1", Email = "new.user1@email.com" };
            var userGroup1 = new UserGroup { Id = "Group1", Name = "New Group1" };
            var userGroupUser1 = new UserGroupUser(userGroup1, user1);

            var user2 = new User { Id = "User2", Name = "New User2", UserName = "new.user2", Email = "new.user2@email.com" };
            var userGroupUser2 = new UserGroupUser(userGroup1, user2);

            var superUser = new User { Id = "superUser", Name = "Super", UserGroupUsers = new[] { userGroupUser1, userGroupUser2 } };

            userRepositoryMoq.Setup(s => s.GetById(It.IsAny<string>())).ReturnsAsync(superUser);

            var userClientServiceMoq = new UserClientService(
                userRepositoryMoq.Object,
                authorizationServiceMoq.Object,
                userValidatorMoq.Object,
                passwordValidatorMoq.Object,
                busMoq.Object,
                settingsMoq.Object,
                Mock.Of<IDatabaseConnectionUserModifier>(),
                Mock.Of<IArcUserXmlWriter>(),
                fabricaUsuarioMoq.Object);

            var retornoUserGroups = await userClientServiceMoq.GetUserGroups("superUser");

            Assert.NotNull(retornoUserGroups);
            Assert.Equal("Group1", retornoUserGroups.Select(s => s.Id).FirstOrDefault());
        }

        [Fact]
        public async Task DissociateFromUserGroup_PublishesEvent()
        {
            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();
            var fabricaUsuarioMoq = new Mock<IFabricaUsuario>();

            var user = new User { Id = "User1", UserName = "new.user1" };
            authorizationServiceMoq.Setup(s => s.DissociateUserFromUserGroup(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(user);

            settingsMoq.Setup(s => s.AuthenticationType).Returns(SettingsAuthenticationType.User);

            fabricaUsuarioMoq.Setup(s => s.MapearParaDtoSaidaUsuario(It.IsAny<User>()))
                .Returns(new OutputUserDto { Id = user.Id, Login = user.UserName, AuthenticationType = AuthenticationType.ActiveDirectory });

            busMoq.Setup(s => s.Publish(It.IsAny<UserCreatedOrUpdatedEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var userClientServiceMoq = new UserClientService(
                userRepositoryMoq.Object,
                authorizationServiceMoq.Object,
                userValidatorMoq.Object,
                passwordValidatorMoq.Object,
                busMoq.Object,
                settingsMoq.Object,
                Mock.Of<IDatabaseConnectionUserModifier>(),
                Mock.Of<IArcUserXmlWriter>(),
                fabricaUsuarioMoq.Object);

            await userClientServiceMoq.DissociateFromUserGroup(user.Id, null, null);

            busMoq.Verify(v => v.Publish(It.IsAny<UserCreatedOrUpdatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AssociateToUserGroup_PublishesEvent()
        {
            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();
            var fabricaUsuarioMoq = new Mock<IFabricaUsuario>();

            var user = new User { Id = "User1", UserName = "new.user1" };
            authorizationServiceMoq.Setup(s => s.AssociateUserToUserGroup(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(user);

            settingsMoq.Setup(s => s.AuthenticationType).Returns(SettingsAuthenticationType.User);

            fabricaUsuarioMoq.Setup(s => s.MapearParaDtoSaidaUsuario(It.IsAny<User>()))
                .Returns(new OutputUserDto { Id = user.Id, Login = user.UserName, AuthenticationType = AuthenticationType.ActiveDirectory });

            busMoq.Setup(s => s.Publish(It.IsAny<UserCreatedOrUpdatedEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var userClientServiceMoq = new UserClientService(
                userRepositoryMoq.Object,
                authorizationServiceMoq.Object,
                userValidatorMoq.Object,
                passwordValidatorMoq.Object,
                busMoq.Object,
                settingsMoq.Object,
                Mock.Of<IDatabaseConnectionUserModifier>(),
                Mock.Of<IArcUserXmlWriter>(),
                fabricaUsuarioMoq.Object);

            await userClientServiceMoq.AssociateToUserGroup(user.Id, null, null);

            busMoq.Verify(v => v.Publish(It.IsAny<UserCreatedOrUpdatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateUserClientService()
        {
            var inputUserDto = new InputUserDto
            {
                Name = "UserClient",
                Login = "login",
                PasswordExpiration = DateTimeOffset.UtcNow,
                ArcXml = "",
                Password = "password",
                AuthenticationType = AuthenticationType.DatabaseUser
            };

            var currentUser = new User
            {
                Id = "id",
                UserName = "login",
                UserGroupUsers = new List<UserGroupUser>(),
                UserSubstitutions = new List<UserSubstitution>(),
                PasswordHistory = "testHistory"
            };

            var mappedUpdatedUser = new User
            {
                Id = currentUser.Id,
                UserName = inputUserDto.Login,
                Name = inputUserDto.Name,
                PasswordHistory = currentUser.PasswordHistory,
                PasswordExpiration = inputUserDto.PasswordExpiration
            };

            var outputUserDto = new OutputUserDto
            {
                Name = inputUserDto.Name,
                Login = inputUserDto.Login,
                PasswordExpiration = inputUserDto.PasswordExpiration
            };

            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var fabricaUsuarioMoq = new Mock<IFabricaUsuario>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();
            var arcUserXmlWriteMoq = new Mock<IArcUserXmlWriter>();

            settingsMoq.Setup(s => s.AuthenticationType).Returns(SettingsAuthenticationType.User);

            userRepositoryMoq.Setup(s => s.GetById(It.IsAny<string>())).ReturnsAsync(currentUser);
            userRepositoryMoq.Setup(s => s.Update(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(mappedUpdatedUser);

            SetMockConfigureParametersToCreateUpdate(userRepositoryMoq);
            userRepositoryMoq.Setup(s => s.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(true);

            fabricaUsuarioMoq
                .Setup(s => s.MapearParaUsuarioAsync(It.IsAny<InputUserDto>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mappedUpdatedUser);
            fabricaUsuarioMoq
                .Setup(s => s.MapearParaDtoSaidaUsuario(It.IsAny<User>()))
                .Returns(outputUserDto);

            busMoq.Setup(s => s.Publish(It.IsAny<UserCreatedOrUpdatedEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var userClientServiceMoq = new UserClientService(
                userRepositoryMoq.Object,
                authorizationServiceMoq.Object,
                userValidatorMoq.Object,
                passwordValidatorMoq.Object,
                busMoq.Object,
                settingsMoq.Object,
                Mock.Of<IDatabaseConnectionUserModifier>(),
                arcUserXmlWriteMoq.Object,
                fabricaUsuarioMoq.Object);

            var savedUser = await userClientServiceMoq.Update(currentUser.Id, inputUserDto, null);

            Assert.NotNull(savedUser);
            Assert.Equal(inputUserDto.Name, savedUser.Name);
            Assert.Equal(inputUserDto.Login, savedUser.Login);
        }

        [Fact]
        public async Task UpdateApiUserClientService()
        {
            var inputUserDto = new InputUserDto
            {
                Name = "UserClient",
                Login = "login",
                PasswordExpiration = DateTimeOffset.UtcNow,
                ArcXml = "",
                Password = "password",
                AuthenticationType = AuthenticationType.DatabaseUser
            };

            var mappedUser = new User
            {
                Name = inputUserDto.Name,
                UserName = inputUserDto.Login
            };

            var outputUserDto = new OutputUserDto
            {
                Name = inputUserDto.Name,
                Login = inputUserDto.Login,
                PasswordExpiration = inputUserDto.PasswordExpiration
            };

            var currentUser = new User
            {
                Id = "id",
                UserName = "login",
                UserGroupUsers = new List<UserGroupUser>(),
                UserSubstitutions = new List<UserSubstitution>(),
                PasswordHistory = "testHistory"
            };

            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var fabricaUsuarioMoq = new Mock<IFabricaUsuario>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();
            var arcUserXmlWriteMoq = new Mock<IArcUserXmlWriter>();

            fabricaUsuarioMoq.Setup(s => s.MapearParaUsuarioAsync(It.IsAny<InputUserDto>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(mappedUser);
            fabricaUsuarioMoq.Setup(s => s.MapearParaDtoSaidaUsuario(It.IsAny<User>())).Returns(outputUserDto);

            userRepositoryMoq.Setup(s => s.GetById(It.IsAny<string>())).ReturnsAsync(currentUser);
            userRepositoryMoq.Setup(s => s.Update(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(currentUser);

            busMoq.Setup(s => s.Publish(It.IsAny<UserCreatedOrUpdatedEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var userClientServiceMoq = new UserClientService(
                userRepositoryMoq.Object,
                authorizationServiceMoq.Object,
                userValidatorMoq.Object,
                passwordValidatorMoq.Object,
                busMoq.Object,
                settingsMoq.Object,
                Mock.Of<IDatabaseConnectionUserModifier>(),
                arcUserXmlWriteMoq.Object,
                fabricaUsuarioMoq.Object);

            var savedUser = await userClientServiceMoq.UpdateApi(currentUser.Id, inputUserDto, null);

            Assert.NotNull(savedUser);
            Assert.Equal(savedUser.Name, inputUserDto.Name);
            Assert.Equal(savedUser.Login, inputUserDto.Login);
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

            var mappedUser = new User
            {
                Name = inputUserDto.Name,
                UserName = inputUserDto.Login
            };

            var outputUserDto = new OutputUserDto
            {
                Name = inputUserDto.Name,
                Login = inputUserDto.Login,
                PasswordExpiration = inputUserDto.PasswordExpiration
            };

            var userRepositoryMoq = new Mock<IUserRepository>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var fabricaUsuarioMoq = new Mock<IFabricaUsuario>();
            var userValidatorMoq = new Mock<IUserValidator>();
            var passwordValidatorMoq = new Mock<IPasswordValidator>();
            var busMoq = new Mock<IBus>();
            var settingsMoq = new Mock<ISettings>();

            fabricaUsuarioMoq
                .Setup(s => s.MapearParaUsuarioAsync(It.IsAny<InputUserDto>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mappedUser);

            fabricaUsuarioMoq
                .Setup(s => s.MapearParaDtoSaidaUsuario(It.IsAny<User>()))
                .Returns(outputUserDto);

            SetMockConfigureParametersToCreateUpdate(userRepositoryMoq);

            userRepositoryMoq.Setup(s => s.Create(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(mappedUser);
            userRepositoryMoq.Setup(s => s.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(true);

            var userClientServiceMoq = new UserClientService(
                userRepositoryMoq.Object,
                authorizationServiceMoq.Object,
                userValidatorMoq.Object,
                passwordValidatorMoq.Object,
                busMoq.Object,
                settingsMoq.Object,
                Mock.Of<IDatabaseConnectionUserModifier>(),
                Mock.Of<IArcUserXmlWriter>(),
                fabricaUsuarioMoq.Object);

            var createdUser = await userClientServiceMoq.CreateApi(inputUserDto, "", null);

            Assert.NotNull(createdUser);
            Assert.Equal(createdUser.Name, inputUserDto.Name);
            Assert.Equal(createdUser.Login, inputUserDto.Login);
        }
    }
}
