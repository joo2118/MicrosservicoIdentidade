using MassTransit;

namespace Identidade.Infraestrutura.Configuracoes.ServiceBus
{
    public interface IConfiguradorEndpoints
    {
        void Configure(IRabbitMqBusFactoryConfigurator busFactoryConfigurator, IBusRegistrationContext context);
        void Configure(IServiceBusBusFactoryConfigurator busFactoryConfigurator, IBusRegistrationContext context);
    }

    public abstract class ConfiguradorEndpoints : IConfiguradorEndpoints
    {
        public static IConfiguradorEndpoints Empty => new ConfiguradorEndpointsVazio();

        public abstract void Configure(IRabbitMqBusFactoryConfigurator busFactoryConfigurator, IBusRegistrationContext context);
        public abstract void Configure(IServiceBusBusFactoryConfigurator busFactoryConfigurator, IBusRegistrationContext context);

        public class ConfiguradorEndpointsVazio : IConfiguradorEndpoints
        {
            public void Configure(IRabbitMqBusFactoryConfigurator busFactoryConfigurator, IBusRegistrationContext context) { }
            public void Configure(IServiceBusBusFactoryConfigurator busFactoryConfigurator, IBusRegistrationContext context) { }
        }
    }
}
