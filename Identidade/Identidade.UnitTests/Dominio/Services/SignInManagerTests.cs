using Xunit;
using NSubstitute;
using Identidade.Dominio.Servicos;
using Identidade.Dominio.Interfaces;
using System.Threading.Tasks;
using Identidade.Dominio.Modelos;
using System;

namespace Identidade.UnitTests.Dominio.Services
{
    public class SignInManagerTests
    {
        [Theory]
        [InlineData(true, true, false)]
        [InlineData(false, true, true)]
        [InlineData(true, false, true)]
        [InlineData(true, true, true)]
        public void Constructor_ThrowsArgumentNullException_WhenDependencyIsNull(
            bool hasUserRepository,
            bool hasLogInService,
            bool shouldThrow)
        {
            IUserRepository userRepository = hasUserRepository ? Substitute.For<IUserRepository>() : null;
            ILogInService logInService = hasLogInService ? Substitute.For<ILogInService>() : null;

            if (shouldThrow)
            {
                Assert.Throws<ArgumentNullException>(() =>
                    new SignInManager(userRepository, logInService));
            }
            else
            {
                var manager = new SignInManager(userRepository, logInService);
                Assert.NotNull(manager);
            }
        }

        [Fact]
        public async Task LogIn_ReturnsTrue_WhenPasswordHashMatches()
        {
            var userRepository = Substitute.For<IUserRepository>();
            var logInService = Substitute.For<ILogInService>();
            var user = new User { PasswordHash = "hashed" };

            userRepository.GetByName("testuser").Returns(Task.FromResult(user));

            var manager = new SignInManager(userRepository, logInService);

            var result = await manager.LogIn("testuser", "password");

            Assert.True(result);
            await logInService.Received(1).LogIn(user);
            await userRepository.DidNotReceive().Update(Arg.Any<User>(), Arg.Any<string>());
        }

        [Fact]
        public async Task LogIn_UpdatesPasswordHashAndReturnsTrue_WhenPasswordHashDoesNotMatchButNewHashDoes()
        {
            var userRepository = Substitute.For<IUserRepository>();
            var logInService = Substitute.For<ILogInService>();
            var user = new User { PasswordHash = "oldhash" };

            userRepository.GetByName("testuser").Returns(Task.FromResult(user));

            var manager = new SignInManager(userRepository, logInService);

            var result = await manager.LogIn("testuser", "password");

            Assert.True(result);
            Assert.Equal("newhash", user.PasswordHash);
            await userRepository.Received(1).Update(user, null);
            await logInService.Received(1).LogIn(user);
        }

        [Fact]
        public async Task LogIn_ReturnsFalse_WhenPasswordVerificationFails()
        {
            var userRepository = Substitute.For<IUserRepository>();
            var logInService = Substitute.For<ILogInService>();
            var user = new User { PasswordHash = "oldhash" };

            userRepository.GetByName("testuser").Returns(Task.FromResult(user));

            var manager = new SignInManager(userRepository, logInService);

            var result = await manager.LogIn("testuser", "password");

            Assert.False(result);
            await userRepository.DidNotReceive().Update(Arg.Any<User>(), Arg.Any<string>());
            await logInService.DidNotReceive().LogIn(Arg.Any<User>());
        }

        [Fact]
        public async Task LogOut_CallsLogOutService()
        {
            var userRepository = Substitute.For<IUserRepository>();
            var logInService = Substitute.For<ILogInService>();

            var manager = new SignInManager(userRepository, logInService);

            await manager.LogOut();

            await logInService.Received(1).LogOut();
        }
    }
}
