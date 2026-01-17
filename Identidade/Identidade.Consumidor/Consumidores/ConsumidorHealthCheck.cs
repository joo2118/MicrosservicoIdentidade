using MassTransit;
using Identidade.Consumidor.Helpers;
using Identidade.Infraestrutura.Interfaces;
using Identidade.Publico.Commands;
using Identidade.Publico.Events;
using System;
using System.Threading.Tasks;
using Identidade.Infraestrutura.Extensoes;
using Microsoft.ApplicationInsights;

namespace Identidade.Consumidor.Consumidores
{
    public class ConsumidorHealthCheck : ConsumidorBase<HealthCheckIdentityCommand>
    {
        private readonly IHealthCheckService _healthCheckService;

        public ConsumidorHealthCheck(IHealthCheckService healthCheckService, IMessageManager messageManager, TelemetryClient telemetryClient)
            : base(messageManager, telemetryClient)
        {
            _healthCheckService = healthCheckService ?? throw new ArgumentNullException(nameof(healthCheckService));
        }

        public override async Task ConsumeContext(ConsumeContext<HealthCheckIdentityCommand> context)
        {
            try
            {
                var values = _healthCheckService.Execute();
                var response = HealthCheckIdentityEvent.FromValues(context.Message.Id, values.ToDto());
                await context.RespondAsync(response);
            }
            catch (Exception e)
            {
                var response = HealthCheckIdentityEvent.FromErrorMessage(context.Message.Id, e.Message);
                await context.RespondAsync(response);
            }
        }
    }
}
