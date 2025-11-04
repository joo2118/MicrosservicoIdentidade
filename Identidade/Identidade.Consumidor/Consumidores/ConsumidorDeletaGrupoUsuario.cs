using MassTransit;
using Identidade.Consumidor.Helpers;
using Identidade.Infraestrutura.ClientServices;
using Identidade.Publico.Commands;
using System.Threading.Tasks;

namespace Identidade.Consumidor.Consumidores
{
    public class ConsumidorDeletaGrupoUsuario : ConsumidorBase<DeleteUserGroupCommand>
    {
        private readonly IUserGroupClientService _userGroupService;

        public ConsumidorDeletaGrupoUsuario(IUserGroupClientService userGroupService, IMessageManager messageManager)
        : base(messageManager)
        {
            _userGroupService = userGroupService;
        }
        
        public override async Task ConsumeContext(ConsumeContext<DeleteUserGroupCommand> context) =>
            await _userGroupService.Delete(context.Message.UserGroupId, context.Message.RequestUserId);
    }
}
