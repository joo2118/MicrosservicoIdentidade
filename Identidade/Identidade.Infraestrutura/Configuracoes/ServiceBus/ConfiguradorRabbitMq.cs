using MassTransit;

namespace Identidade.Infraestrutura.Configuracoes.ServiceBus
{
    public class ConfiguradorRabbitMq : IConfiguradorMessageBroker
    {
        public void Configure(IBusRegistrationConfigurator x, ISettings settings, IConfiguradorEndpoints receiveEndpointsConfigurator)
        {
            x.UsingRabbitMq((context, cfg) => ConfigureRabbitMq(cfg, context, settings, receiveEndpointsConfigurator));
        }

        public static void ConfigureRabbitMq(IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context, ISettings settings, IConfiguradorEndpoints receiveEndpointsConfigurator)
        {
            cfg.Host(settings.RabbitMQ.Host, settings.RabbitMQ.VirtualHost, h =>
            {
                h.Username(settings.RabbitMQ.UserName);
                h.Password(settings.RabbitMQ.Password);
            });

            cfg.UseNewtonsoftJsonSerializer();
            receiveEndpointsConfigurator.Configure(cfg, context);
            cfg.ConfigureEndpoints(context);
        }
    }
}