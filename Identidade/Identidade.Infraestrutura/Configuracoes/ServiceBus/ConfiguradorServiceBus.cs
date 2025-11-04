using MassTransit;
using Identidade.Publico.Enumerations;
using System.Collections.Generic;

namespace Identidade.Infraestrutura.Configuracoes.ServiceBus
{
    public static class ConfiguradorServiceBus
    {
        private static readonly IDictionary<MessageBroker, IConfiguradorMessageBroker> _configurators =
            new Dictionary<MessageBroker, IConfiguradorMessageBroker>
            {
                [MessageBroker.AzureServiceBus] = new ConfiguradorAzureServiceBus(),
                [MessageBroker.RabbitMQ] = new ConfiguradorRabbitMq()
            };

        public static void Configure(IBusRegistrationConfigurator busRegistrationConfigurator, ISettings settings, IConfiguradorEndpoints receiveEndpointsConfigurator)
        {
            var configurator = GetConfigurator(settings.MessageBroker);
            configurator.Configure(busRegistrationConfigurator, settings, receiveEndpointsConfigurator);
        }

        private static IConfiguradorMessageBroker GetConfigurator(MessageBroker messageBroker)
        {
            _configurators.TryGetValue(messageBroker, out var configurator);
            return configurator ?? ConfiguradorMessageBroker.Empty;
        }
    }
}