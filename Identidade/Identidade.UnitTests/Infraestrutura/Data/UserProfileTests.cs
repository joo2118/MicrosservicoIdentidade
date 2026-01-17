using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Modelos;
using Identidade.Infraestrutura.Data;
using Identidade.Publico.Dtos;
using Identidade.Publico.Enumerations;
using System;
using Xunit;

namespace Identidade.UnitTests.Infraestrutura.Data
{
    public class UserProfileTests
    {
        [Fact]
        public void Map_InputUserDtoToUser()
        {
            var password = "password";
            var userGroup = "group1";
            var substituteUser = "subUser1";
            var login = "testUser";

            var inputUserDto = new InputUserDto
            {
                Login = login,
                Password = password,
                Email = "test@example.com",
                UserGroups = new string[] { userGroup },
                SubstituteUsers = new string[] { substituteUser }
            };

            
            var mockUserRepository = Substitute.For<IReadOnlyRepository<User>>();
            var mockUserGroupRepository = Substitute.For<IUserGroupRepository>();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile(mockUserRepository, mockUserGroupRepository));
            });

            var mapper = configuration.CreateMapper();

            mockUserRepository.GetByName(Arg.Any<string>()).Returns(new User());
            mockUserGroupRepository.GetUsers(inputUserDto.SubstituteUsers).Returns(new User[] { new User() { Id = substituteUser } });
            mockUserGroupRepository.GetUserGroups(inputUserDto.UserGroups).Returns(new UserGroup[] { new UserGroup() { Id = userGroup } });

            var user = mapper.Map<User>(inputUserDto);

            Assert.NotNull(user.UserGroupUsers);
            Assert.Single(user.UserGroupUsers);
            Assert.NotNull(user.UserSubstitutions);
            Assert.Single(user.UserSubstitutions);

            Assert.Equal(password, user.PasswordHash);
            Assert.Equal(login, user.UserName);
            Assert.Equal(inputUserDto.Email, user.Email);
        }

        [Fact]
        public void Map_InputUserDtoToUser_InactiveAndNoUserGroupsAndSubstitute()
        {
            var password = "password";
            var login = "testUser";

            var inputUserDto = new InputUserDto
            {
                Login = login,
                Password = password,
                Email = "test@example.com",
                Active = false
            };

            
            var mockUserRepository = Substitute.For<IReadOnlyRepository<User>>();
            var mockUserGroupRepository = Substitute.For<IUserGroupRepository>();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile(mockUserRepository, mockUserGroupRepository));
            });

            var mapper = configuration.CreateMapper();

            
            mockUserRepository.GetByName(Arg.Any<string>()).Returns(new User());

            var user = mapper.Map<User>(inputUserDto);

            Assert.Empty(user.UserGroupUsers);
            Assert.Empty(user.UserSubstitutions);
            Assert.Equal(DateTimeOffset.MaxValue, user.LockoutEnd);

            Assert.Equal(password, user.PasswordHash);
            Assert.Equal(login, user.UserName);
            Assert.Equal(inputUserDto.Email, user.Email);

            Assert.Equal(inputUserDto.Login, user.UserName);
            Assert.Equal(password, user.PasswordHash);
            Assert.Equal(inputUserDto.Email, user.Email);
        }

        [Fact]
        public void Map_InputUserDtoToUser_GetUserGroupException()
        {
            var password = "password";
            var login = "testUser";
            var expectedMessage = $"There's no user with the ID 'userGroup' on the database.";

            var inputUserDto = new InputUserDto
            {
                Login = login,
                Password = password,
                Email = "test@example.com",
                UserGroups = new string[] { "userGroup" },
                SubstituteUsers = new string[] { "substituteUser" },
            };

            
            var mockUserRepository = Substitute.For<IReadOnlyRepository<User>>();
            var mockUserGroupRepository = Substitute.For<IUserGroupRepository>();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile(mockUserRepository, mockUserGroupRepository));
            });

            var mapper = configuration.CreateMapper();

            
            mockUserRepository.GetByName(Arg.Any<string>()).Returns(new User());

            mockUserGroupRepository.GetUsers(Arg.Any<string[]>()).Returns(new User[] { new User() });
            mockUserGroupRepository.GetUserGroups(Arg.Any<string[]>()).Throws(new Exception(expectedMessage));

            var exception = Assert.Throws<AutoMapperMappingException>(() => mapper.Map<User>(inputUserDto));
            Assert.Equal(expectedMessage, exception.InnerException?.Message);
        }

        [Fact]
        public void Map_InputUserDtoToUser_GetUserException()
        {
            var password = "password";
            var login = "testUser";
            var expectedMessage = $"There's no user with the ID 'substituteUser' on the database.";

            var inputUserDto = new InputUserDto
            {
                Login = login,
                Password = password,
                Email = "test@example.com",
                UserGroups = new string[] { "userGroup" },
                SubstituteUsers = new string[] { "substituteUser" },
            };

            
            var mockUserRepository = Substitute.For<IReadOnlyRepository<User>>();
            var mockUserGroupRepository = Substitute.For<IUserGroupRepository>();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile(mockUserRepository, mockUserGroupRepository));
            });

            var mapper = configuration.CreateMapper();
            
            mockUserGroupRepository.GetUsers(Arg.Any<string[]>()).Throws(new Exception(expectedMessage));
            mockUserGroupRepository.GetUserGroups(Arg.Any<string[]>()).Returns(new UserGroup[] { new UserGroup() });
            mockUserRepository.GetByName(Arg.Any<string>()).Returns(new User());

            mockUserRepository.GetById(Arg.Any<string>()).Throws(new Exception(expectedMessage));

            var exception = Assert.Throws<AutoMapperMappingException>(() => mapper.Map<User>(inputUserDto));
            Assert.Equal(expectedMessage, exception.InnerException?.Message);
        }

        [Fact]
        public void Map_ArcUserDtoToUser()
        {
            var password = "password";
            var userGroup = "group1";
            var substituteUser = "subUser1";
            var login = "testUser";
            var email = "test@example.com";

            var arcUserDto = new ArcUserDto(email, password, null, null, null, AuthenticationType.DatabaseUser, null)
            {
                Login = login,
                UserGroups = new string[] { userGroup },
                SubstituteUsers = new string[] { substituteUser },
                Active = true
            };

            
            var mockUserRepository = Substitute.For<IReadOnlyRepository<User>>();
            var mockUserGroupRepository = Substitute.For<IUserGroupRepository>();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile(mockUserRepository, mockUserGroupRepository));
            });

            var mapper = configuration.CreateMapper();

            
            mockUserRepository.GetByName(Arg.Any<string>()).Returns(new User());
            mockUserGroupRepository.GetUsers(arcUserDto.SubstituteUsers).Returns(new User[] { new User() { Id = substituteUser } });
            mockUserGroupRepository.GetUserGroups(arcUserDto.UserGroups).Returns(new UserGroup[] { new UserGroup() { Id = userGroup } });

            var user = mapper.Map<User>(arcUserDto);

            Assert.NotNull(user.UserGroupUsers);
            Assert.Single(user.UserGroupUsers);
            Assert.NotNull(user.UserSubstitutions);
            Assert.Single(user.UserSubstitutions);
            Assert.Equal(DateTimeOffset.MinValue, user.LockoutEnd);

            Assert.Equal(arcUserDto.Login, user.UserName);
            Assert.Equal(password, user.PasswordHash);
            Assert.Equal(arcUserDto.Email, user.Email);
        }

        [Fact]
        public void Map_ArcUserDtoToUser_InactiveAndNoUserGroupsAndSubstitute()
        {
            var password = "password";
            var login = "testUser";
            var email = "test@example.com";

            var arcUserDto = new ArcUserDto(email, password, null, null, null, AuthenticationType.DatabaseUser, null)
            {
                Login = login,
                Active = false,
            };

            
            var mockUserRepository = Substitute.For<IReadOnlyRepository<User>>();
            var mockUserGroupRepository = Substitute.For<IUserGroupRepository>();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile(mockUserRepository, mockUserGroupRepository));
            });

            var mapper = configuration.CreateMapper();

            
            mockUserRepository.GetByName(Arg.Any<string>()).Returns(new User());

            var user = mapper.Map<User>(arcUserDto);

            Assert.Empty(user.UserGroupUsers);
            Assert.Empty(user.UserSubstitutions);
            Assert.Equal(DateTimeOffset.MaxValue, user.LockoutEnd);

            Assert.Equal(arcUserDto.Login, user.UserName);
            Assert.Equal(password, user.PasswordHash);
            Assert.Equal(arcUserDto.Email, user.Email);
        }

        [Fact]
        public void Map_ArcUserDtoToUser_GetUserGroupException()
        {
            var password = "password";
            var email = "test@example.com";
            var expectedMessage = $"There's no user with the ID 'userGroup' on the database.";

            var arcUserDto = new ArcUserDto(email, password, null, null, null, AuthenticationType.DatabaseUser, null)
            {
                Login = "login",
                UserGroups = new string[] { "userGroup" },
                SubstituteUsers = new string[] { "substituteUser" },
            };

            
            var mockUserRepository = Substitute.For<IReadOnlyRepository<User>>();
            var mockUserGroupRepository = Substitute.For<IUserGroupRepository>();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile(mockUserRepository, mockUserGroupRepository));
            });

            var mapper = configuration.CreateMapper();

            
            mockUserRepository.GetByName(Arg.Any<string>()).Returns(new User());

            mockUserGroupRepository.GetUsers(Arg.Any<string[]>()).Returns(new User[] { new User() });
            mockUserGroupRepository.GetUserGroups(Arg.Any<string[]>()).Throws(new Exception(expectedMessage));

            var exception = Assert.Throws<AutoMapperMappingException>(() => mapper.Map<User>(arcUserDto));
            Assert.Equal(expectedMessage, exception.InnerException?.Message);
        }

        [Fact]
        public void Map_ArcUserDtoToUser_GetUserException()
        {
            var password = "password";
            var email = "test@example.com";
            var expectedMessage = $"There's no user with the ID 'substituteUser' on the database.";

            var arcUserDto = new ArcUserDto(email, password, null, null, null, AuthenticationType.DatabaseUser, null)
            {
                Login = "login",
                UserGroups = new string[] { "userGroup" },
                SubstituteUsers = new string[] { "substituteUser" },
            };

            
            var mockUserRepository = Substitute.For<IReadOnlyRepository<User>>();
            var mockUserGroupRepository = Substitute.For<IUserGroupRepository>();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile(mockUserRepository, mockUserGroupRepository));
            });

            var mapper = configuration.CreateMapper();

            
            mockUserGroupRepository.GetUsers(Arg.Any<string[]>()).Throws(new Exception(expectedMessage));
            mockUserGroupRepository.GetUserGroups(Arg.Any<string[]>()).Returns(new UserGroup[] { new UserGroup() });
            mockUserRepository.GetByName(Arg.Any<string>()).Returns(new User());

            mockUserRepository.GetById(Arg.Any<string>()).Throws(new Exception(expectedMessage));

            var exception = Assert.Throws<AutoMapperMappingException>(() => mapper.Map<User>(arcUserDto));
            Assert.Equal(expectedMessage, exception.InnerException?.Message);
        }
    }
}