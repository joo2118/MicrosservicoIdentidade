using MassTransit;
using Identidade.Consumidor.Helpers;
using Identidade.Publico.Commands;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Identidade.Infraestrutura.ServicosCliente;

namespace Identidade.Consumidor.Consumidores
{
    public class ConsumidorDeletaGrupoUsuario : ConsumidorBase<DeleteUserGroupCommand>
    {
        private readonly IUserGroupClientService _userGroupService;

        public ConsumidorDeletaGrupoUsuario(IUserGroupClientService userGroupService, IMessageManager messageManager, TelemetryClient telemetryClient)
        : base(messageManager, telemetryClient)
        {
            _userGroupService = userGroupService;
        }
        
        public override async Task ConsumeContext(ConsumeContext<DeleteUserGroupCommand> context) =>
            await _userGroupService.Delete(context.Message.UserGroupId, context.Message.RequestUserId);
    }
}
