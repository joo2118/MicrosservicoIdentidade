using MassTransit;
using Identidade.Consumidor.Helpers;
using Identidade.Dominio.Helpers;
using Identidade.Publico.Commands;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Identidade.Infraestrutura.ServicosCliente;
using System.Collections.Generic;

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

        protected override string GetCommandName()
            => nameof(CreateOrUpdateUserCommand);

        protected override Dictionary<string, string> GetErrorMetadata(ConsumeContext<CreateOrUpdateUserCommand> context)
            => new()
            {
                ["UserId"] = context.Message.UserId,
                ["UserLogin"] = context.Message.User?.Login,
                ["RequestUserId"] = context.Message.RequestUserId
            };

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
