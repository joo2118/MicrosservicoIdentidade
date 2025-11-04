using System.Threading.Tasks;
using MassTransit;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Identidade.Consumidor.Consumidores;
using Identidade.Consumidor.Helpers;
using Identidade.Dominio.Helpers;
using Identidade.Infraestrutura.ClientServices;
using Identidade.Publico.Commands;
using Identidade.Publico.Dtos;
using Xunit;

namespace Identidade.Consumidor.Tests
{
    public class CreateOrUpdateUserConsumerTests
    {
        [Fact]
        public async Task ConsumeContext_UserExists_CallsUpdateOnUserService()
        {
            var userService = Substitute.For<IUserClientService>();
            var messageManager = Substitute.For<IMessageManager>();
            var context = Substitute.For<ConsumeContext<CreateOrUpdateUserCommand>>();
            var command = new CreateOrUpdateUserCommand
            {
                UserId = "test-user-id",
                User = new InputUserDto { Name = "Test User" },
                RequestUserId = "request-user-id"
            };
            context.Message.Returns(command);

            userService.GetById("test-user-id").Returns(new OutputUserDto());

            var consumer = new ConsumidorCriaOuAtualizaUsuario(userService, messageManager);

            await consumer.ConsumeContext(context);

            await userService.Received(1).Update("test-user-id", command.User, "request-user-id");
            await userService.DidNotReceive().Create(Arg.Any<InputUserDto>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task ConsumeContext_UserDoesNotExist_CallsCreateOnUserService()
        {
            var userService = Substitute.For<IUserClientService>();
            var messageManager = Substitute.For<IMessageManager>();
            var context = Substitute.For<ConsumeContext<CreateOrUpdateUserCommand>>();
            var command = new CreateOrUpdateUserCommand
            {
                UserId = "test-user-id",
                User = new InputUserDto { Name = "Test User" },
                RequestUserId = "request-user-id"
            };
            context.Message.Returns(command);

            userService.GetById("test-user-id").Throws(new NotFoundAppException());

            var consumer = new ConsumidorCriaOuAtualizaUsuario(userService, messageManager);

            await consumer.ConsumeContext(context);

            await userService.Received(1).Create(command.User, "request-user-id", "test-user-id");
            await userService.DidNotReceive().Update(Arg.Any<string>(), Arg.Any<InputUserDto>(), Arg.Any<string>());
        }
    }
}
