using MassTransit;

namespace Identidade.Infraestrutura.Configuracoes.ServiceBus
{
    public class ConfiguradorAzureServiceBus : IConfiguradorMessageBroker
    {
        public void Configure(IBusRegistrationConfigurator x, ISettings settings, IConfiguradorEndpoints receiveEndpointsConfigurator)
        {
            x.UsingAzureServiceBus((context, cfg) => ConfigureAzureServiceBus(cfg, context, settings, receiveEndpointsConfigurator));
        }

        public static void ConfigureAzureServiceBus(IServiceBusBusFactoryConfigurator cfg, IBusRegistrationContext context, ISettings settings, IConfiguradorEndpoints receiveEndpointsConfigurator)
        {
            cfg.Host(settings.AzureServiceBus.ConnectionString);

            cfg.UseNewtonsoftJsonSerializer();

            receiveEndpointsConfigurator.Configure(cfg, context);

            cfg.ConfigureEndpoints(context);
        }
    }
}