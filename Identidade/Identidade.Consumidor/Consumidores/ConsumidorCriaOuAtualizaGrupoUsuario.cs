using MassTransit;
using Identidade.Consumidor.Helpers;
using Identidade.Dominio.Helpers;
using Identidade.Infraestrutura.ClientServices;
using Identidade.Publico.Commands;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;

namespace Identidade.Consumidor.Consumidores
{
    public class ConsumidorCriaOuAtualizaGrupoUsuario : ConsumidorBase<CreateOrUpdateUserGroupCommand>
    {
        private readonly IUserGroupClientService _userGroupService;

        public ConsumidorCriaOuAtualizaGrupoUsuario(IUserGroupClientService userGroupService, IMessageManager messageManager, TelemetryClient telemetryClient)
            : base(messageManager, telemetryClient)
        {
            _userGroupService = userGroupService;
        }

        public override async Task ConsumeContext(ConsumeContext<CreateOrUpdateUserGroupCommand> context)
        {
            var message = context.Message;
            var userGroupId = message.UserGroupId;
            var userGroup = message.UserGroup;
            var requestUserId = message.RequestUserId;

            try
            {
                _userGroupService.GetById(userGroupId).GetAwaiter().GetResult();
                await _userGroupService.Update(userGroupId, userGroup, requestUserId);
            }
            catch (NotFoundAppException)
            {
                await _userGroupService.Create(userGroup, requestUserId, userGroupId);
            }
        }
    }
}
