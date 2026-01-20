using System;
using System.Threading.Tasks;
using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Modelos;
using Identidade.Dominio.Servicos;
using Identidade.Publico.Dtos;
using NSubstitute;
using Xunit;

namespace Identidade.UnitTests.Dominio.Servicos
{
    public class FabricaUsuarioTests
    {
        [Fact]
        public async Task Map_InputUserDtoToUser_Builds_Collections_And_Maps_Basics()
        {
            var userGroupId = "group1";
            var substituteUserId = "subUser1";
            var login = "testUser";

            var inputUserDto = new InputUserDto
            {
                Login = login,
                Password = "password",
                Email = "test@example.com",
                UserGroups = new[] { userGroupId },
                SubstituteUsers = new[] { substituteUserId }
            };

            var userRepo = Substitute.For<IReadOnlyRepository<User>>();
            var userGroupRepo = Substitute.For<IUserGroupRepository>();

            userGroupRepo.GetUsers(Arg.Any<string[]>()).Returns(new[] { new User { Id = substituteUserId } });
            userGroupRepo.GetUserGroups(Arg.Any<string[]>()).Returns(new[] { new UserGroup { Id = userGroupId } });

            var fabrica = new FabricaUsuario(userRepo, userGroupRepo);

            var user = await fabrica.MapearParaUsuarioAsync(inputUserDto);

            Assert.Equal(login, user.UserName);
            Assert.Equal(inputUserDto.Email, user.Email);
            Assert.Equal(inputUserDto.Password, user.PasswordHash);

            Assert.NotNull(user.UserGroupUsers);
            Assert.Single(user.UserGroupUsers);
            Assert.Equal(userGroupId, ((System.Linq.Enumerable.FirstOrDefault(user.UserGroupUsers)) as UserGroupUser)?.UserGroupId);

            Assert.NotNull(user.UserSubstitutions);
            Assert.Single(user.UserSubstitutions);
        }

        [Fact]
        public async Task Map_InputUserDto_Inactive_Sets_LockoutEnd_MaxValue()
        {
            var userRepo = Substitute.For<IReadOnlyRepository<User>>();
            var userGroupRepo = Substitute.For<IUserGroupRepository>();
            var fabrica = new FabricaUsuario(userRepo, userGroupRepo);

            var dto = new InputUserDto { Login = "u", Active = false };
            var user = await fabrica.MapearParaUsuarioAsync(dto);

            Assert.Equal(DateTimeOffset.MaxValue, user.LockoutEnd);
        }
    }
}
