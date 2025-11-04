using System.Threading.Tasks;
using MassTransit;
using NSubstitute;
using Identidade.Consumidor.Consumidores;
using Identidade.Consumidor.Helpers;
using Identidade.Infraestrutura.ClientServices;
using Identidade.Publico.Commands;
using Xunit;

namespace Identidade.UnitTests.Consumers
{
    public class DeleteUserGroupConsumerTests
    {
        [Fact]
        public async Task ConsumeContext_CallsDeleteOnUserGroupService()
        {
            var userGroupService = Substitute.For<IUserGroupClientService>();
            var messageManager = Substitute.For<IMessageManager>();
            var context = Substitute.For<ConsumeContext<DeleteUserGroupCommand>>();
            var command = new DeleteUserGroupCommand { UserGroupId = "test-group-id", RequestUserId = "request-user-id" };
            context.Message.Returns(command);

            var consumer = new ConsumidorDeletaGrupoUsuario(userGroupService, messageManager);

            await consumer.ConsumeContext(context);

            await userGroupService.Received(1).Delete("test-group-id", "request-user-id");
        }
    }
}
