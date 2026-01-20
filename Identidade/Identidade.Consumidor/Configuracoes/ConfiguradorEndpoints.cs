using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Identidade.Consumidor.Consumidores;
using Identidade.Infraestrutura.RedisNotifier;
using Identidade.Dominio.Interfaces;
using Identidade.Infraestrutura.Configuracoes.ServiceBus;
using Identidade.Dominio.Modelos;
using System;

namespace Identidade.Consumidor.Configuracoes
{
    public class ConfiguradorEndpoints : IConfiguradorEndpoints
    {
        public void Configure(IRabbitMqBusFactoryConfigurator busFactoryConfigurator, IBusRegistrationContext context)
        {
            ConfigureEndpoints(busFactoryConfigurator, context);
        }

        public void Configure(IServiceBusBusFactoryConfigurator busFactoryConfigurator, IBusRegistrationContext context)
        {
            ConfigureEndpoints(busFactoryConfigurator, context);
        }

        private static void ConfigureEndpoints<T>(T busFactoryConfigurator, IBusRegistrationContext context)
            where T : IBusFactoryConfigurator
        {
            busFactoryConfigurator.UseConcurrencyLimit(1);
            AddReceiveEndPoint<ConsumidorCriaOuAtualizaUsuario>(busFactoryConfigurator, context);
            AddReceiveEndPoint<ConsumidorDeletaUsuario>(busFactoryConfigurator, context);
            AddReceiveEndPoint<ConsumidorCriaOuAtualizaGrupoUsuario>(busFactoryConfigurator, context);
            AddReceiveEndPoint<ConsumidorDeletaGrupoUsuario>(busFactoryConfigurator, context);
            AddReceiveEndPoint<ConsumidorHealthCheck>(busFactoryConfigurator, context, Constants.cst_HealthCheck_Queue);
        }

        private static void AddReceiveEndPoint<T>(IBusFactoryConfigurator busFactoryConfigurator, IBusRegistrationContext context, string queueName = null)
            where T : class, IConsumer
        {
            busFactoryConfigurator.ReceiveEndpoint(string.IsNullOrWhiteSpace(queueName) ? typeof(T).Name : queueName, ep =>
            {
                ep.UseConcurrencyLimit(1);

                ep.UseMessageRetry(r => r.Exponential(
                    retryLimit: 5,
                    minInterval: TimeSpan.FromSeconds(1),
                    maxInterval: TimeSpan.FromSeconds(30),
                    intervalDelta: TimeSpan.FromSeconds(2)));

                ep.ConfigureConsumer<T>(context, consumerCfg =>
                {
                    consumerCfg.UseConcurrencyLimit(1);
                    consumerCfg.UseFilter(new PipelineFilter<T>(context.GetService<IRedisStatusNotifier>()));
                });
            });
        }
    }
}