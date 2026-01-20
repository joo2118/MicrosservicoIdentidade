using MassTransit;
using Identidade.Consumidor.Helpers;
using Identidade.Publico.Commands;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Identidade.Infraestrutura.ServicosCliente;

namespace Identidade.Consumidor.Consumidores
{
    public class ConsumidorDeletaUsuario : ConsumidorBase<DeleteUserCommand>
    {
        private readonly IUserClientService _userService;

        public ConsumidorDeletaUsuario(IUserClientService userService, IMessageManager messageManager, TelemetryClient telemetryClient)
        : base(messageManager, telemetryClient)
        {
            _userService = userService;
        }

        public override async Task ConsumeContext(ConsumeContext<DeleteUserCommand> context) =>
            await _userService.Delete(context.Message.UserId, context.Message.RequestUserId);
    }
}
