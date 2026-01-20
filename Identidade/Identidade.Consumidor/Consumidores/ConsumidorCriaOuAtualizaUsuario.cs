using MassTransit;
using Identidade.Consumidor.Helpers;
using Identidade.Dominio.Helpers;
using Identidade.Publico.Commands;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Identidade.Infraestrutura.ServicosCliente;

namespace Identidade.Consumidor.Consumidores
{
    public class ConsumidorCriaOuAtualizaUsuario : ConsumidorBase<CreateOrUpdateUserCommand>
    {
        private readonly IUserClientService _userService;

        public ConsumidorCriaOuAtualizaUsuario(IUserClientService userService, IMessageManager messageManager, TelemetryClient telemetryClient)
            : base(messageManager, telemetryClient)
        {
            _userService = userService;
        }

        public override async Task ConsumeContext(ConsumeContext<CreateOrUpdateUserCommand> context)
        {
            var message = context.Message;
            var userId = message.UserId;
            var user = message.User;
            var requestUserId = message.RequestUserId;

            try
            {
                _userService.GetById(userId).GetAwaiter().GetResult();
                await _userService.Update(userId, user, requestUserId);
            }
            catch (NotFoundAppException)
            {
                await _userService.Create(user, requestUserId, userId);
            }
        }
    }
}
