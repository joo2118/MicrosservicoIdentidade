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

namespace Identidade.UnitTests.Consumers
{
    public class CreateOrUpdateUserGroupConsumerTests
    {
        [Fact]
        public async Task ConsumeContext_GroupExists_CallsUpdateOnUserGroupService()
        {
            var userGroupService = Substitute.For<IUserGroupClientService>();
            var messageManager = Substitute.For<IMessageManager>();
            var context = Substitute.For<ConsumeContext<CreateOrUpdateUserGroupCommand>>();
            var command = new CreateOrUpdateUserGroupCommand
            {
                UserGroupId = "test-group-id",
                UserGroup = new InputUserGroupDto { Name = "Test Group" },
                RequestUserId = "request-user-id"
            };
            context.Message.Returns(command);

            userGroupService.GetById("test-group-id").Returns(new OutputUserGroupDto());

            var consumer = new ConsumidorCriaOuAtualizaGrupoUsuario(userGroupService, messageManager);

            await consumer.ConsumeContext(context);

            await userGroupService.Received(1).Update("test-group-id", command.UserGroup, "request-user-id");
            await userGroupService.DidNotReceive().Create(Arg.Any<InputUserGroupDto>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task ConsumeContext_GroupDoesNotExist_CallsCreateOnUserGroupService()
        {
            var userGroupService = Substitute.For<IUserGroupClientService>();
            var messageManager = Substitute.For<IMessageManager>();
            var context = Substitute.For<ConsumeContext<CreateOrUpdateUserGroupCommand>>();
            var command = new CreateOrUpdateUserGroupCommand
            {
                UserGroupId = "test-group-id",
                UserGroup = new InputUserGroupDto { Name = "Test Group" },
                RequestUserId = "request-user-id"
            };
            context.Message.Returns(command);

            userGroupService.GetById("test-group-id").Throws(new NotFoundAppException());

            var consumer = new ConsumidorCriaOuAtualizaGrupoUsuario(userGroupService, messageManager);

            await consumer.ConsumeContext(context);

            await userGroupService.Received(1).Create(command.UserGroup, "request-user-id", "test-group-id");
            await userGroupService.DidNotReceive().Update(Arg.Any<string>(), Arg.Any<InputUserGroupDto>(), Arg.Any<string>());
        }
    }
}
