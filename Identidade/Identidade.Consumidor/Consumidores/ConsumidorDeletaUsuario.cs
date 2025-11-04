using MassTransit;
using Identidade.Consumidor.Helpers;
using Identidade.Infraestrutura.ClientServices;
using Identidade.Publico.Commands;
using System.Threading.Tasks;

namespace Identidade.Consumidor.Consumidores
{
    public class ConsumidorDeletaUsuario : ConsumidorBase<DeleteUserCommand>
    {
        private readonly IUserClientService _userService;

        public ConsumidorDeletaUsuario(IUserClientService userService, IMessageManager messageManager)
        : base(messageManager)
        {
            _userService = userService;
        }

        public override async Task ConsumeContext(ConsumeContext<DeleteUserCommand> context) =>
            await _userService.Delete(context.Message.UserId, context.Message.RequestUserId);
    }
}
