using MassTransit;

namespace Identidade.Infraestrutura.Configuracoes.ServiceBus
{
    public interface IConfiguradorMessageBroker
    {
        void Configure(IBusRegistrationConfigurator x, ISettings settings, IConfiguradorEndpoints receiveEndpointsConfigurator);
    }
        
    public abstract class ConfiguradorMessageBroker : IConfiguradorMessageBroker
    {
        public static IConfiguradorMessageBroker Empty => new ConfiguradorMessageBrokerVazio();

        public abstract void Configure(IBusRegistrationConfigurator x, ISettings settings, IConfiguradorEndpoints receiveEndpointsConfigurator);

        public class ConfiguradorMessageBrokerVazio : IConfiguradorMessageBroker
        {
            public void Configure(IBusRegistrationConfigurator x, ISettings settings, IConfiguradorEndpoints receiveEndpointsConfigurator) { }
        }
    }
}
