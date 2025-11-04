using System.Threading.Tasks;
using MassTransit;
using NSubstitute;
using Identidade.Consumidor.Consumidores;
using Identidade.Consumidor.Helpers;
using Identidade.Infraestrutura.ClientServices;
using Identidade.Publico.Commands;
using Xunit;

namespace Identidade.Consumidor.Tests
{
    public class DeleteUserConsumerTests
    {
        [Fact]
        public async Task ConsumeContext_CallsDeleteOnUserService()
        {
            var userService = Substitute.For<IUserClientService>();
            var messageManager = Substitute.For<IMessageManager>();
            var context = Substitute.For<ConsumeContext<DeleteUserCommand>>();
            var command = new DeleteUserCommand { UserId = "test-user-id", RequestUserId = "request-user-id" };
            context.Message.Returns(command);

            var consumer = new ConsumidorDeletaUsuario(userService, messageManager);

            await consumer.ConsumeContext(context);

            await userService.Received(1).Delete("test-user-id", "request-user-id");
        }
    }
}
