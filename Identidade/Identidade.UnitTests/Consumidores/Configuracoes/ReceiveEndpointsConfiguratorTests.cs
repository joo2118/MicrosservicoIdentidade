using MassTransit;
using MassTransit.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Identidade.Consumidor.Consumidores;
using Identidade.Dominio.Interfaces;
using Identidade.Infraestrutura.RedisNotifier;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static Identidade.Infraestrutura.Configuracoes.ServiceBus.ConfiguradorEndpoints;
using ConfiguradorEndpoints = Identidade.Consumidor.Configuracoes.ConfiguradorEndpoints;
using Identidade.Infraestrutura.Configuracoes.ServiceBus;
using Identidade.Consumidor.Consumidores;
using Identidade.Dominio.Modelos;

namespace Identidade.UnitTests.Consumers.Configurations
{
    public class ReceiveEndpointsConfiguratorTests
    {
        private readonly ConfiguradorEndpoints _configurator;
        private readonly IRabbitMqBusFactoryConfigurator _rabbitMqCfg;
        private readonly IServiceBusBusFactoryConfigurator _serviceBusCfg;
        private readonly IBusRegistrationContext _context;
        private readonly IReceiveEndpointConfigurator _endpointConfigurator;
        private readonly IRedisStatusNotifier _redisStatusNotifier;

        public ReceiveEndpointsConfiguratorTests()
        {
            _configurator = new ConfiguradorEndpoints();
            _rabbitMqCfg = Substitute.For<IRabbitMqBusFactoryConfigurator>();
            _serviceBusCfg = Substitute.For<IServiceBusBusFactoryConfigurator>();
            _context = Substitute.For<IBusRegistrationContext>();
            _endpointConfigurator = Substitute.For<IReceiveEndpointConfigurator>();
            _redisStatusNotifier = Substitute.For<IRedisStatusNotifier>();
        }

        #region RabbitMQ Configuration Tests

        [Fact]
        public void Configure_RabbitMq_ShouldConfigureAllConsumerEndpoints()
        {
            var rabbitMqCfg = Substitute.For<IRabbitMqBusFactoryConfigurator>();
            var context = Substitute.For<IBusRegistrationContext>();
            _configurator.Configure(rabbitMqCfg, context);

            (rabbitMqCfg as IBusFactoryConfigurator).Received(1).ReceiveEndpoint(nameof(ConsumidorCriaOuAtualizaUsuario), Arg.Any<Action<IReceiveEndpointConfigurator>>());
            (rabbitMqCfg as IBusFactoryConfigurator).Received(1).ReceiveEndpoint(nameof(ConsumidorDeletaUsuario), Arg.Any<Action<IReceiveEndpointConfigurator>>());
            (rabbitMqCfg as IBusFactoryConfigurator).Received(1).ReceiveEndpoint(nameof(ConsumidorCriaOuAtualizaGrupoUsuario), Arg.Any<Action<IReceiveEndpointConfigurator>>());
            (rabbitMqCfg as IBusFactoryConfigurator).Received(1).ReceiveEndpoint(nameof(ConsumidorDeletaGrupoUsuario), Arg.Any<Action<IReceiveEndpointConfigurator>>());
            (rabbitMqCfg as IBusFactoryConfigurator).Received(1).ReceiveEndpoint(Constants.cst_HealthCheck_Queue, Arg.Any<Action<IReceiveEndpointConfigurator>>());
        }

        [Fact]
        public void Configure_RabbitMq_WithNullContext_ShouldNotThrow()
        {
            var exception = Record.Exception(() => _configurator.Configure(_rabbitMqCfg, null));
            Assert.Null(exception);
        }

        [Fact]
        public void Configure_RabbitMq_WithNullConfigurator_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => _configurator.Configure((IRabbitMqBusFactoryConfigurator)null, _context));
        }

        #endregion

        #region Service Bus Configuration Tests

        [Fact]
        public void Configure_ServiceBus_ShouldConfigureAllConsumerEndpoints()
        {
            var serviceBusCfg = Substitute.For<IServiceBusBusFactoryConfigurator>();
            var context = Substitute.For<IBusRegistrationContext>();
            _configurator.Configure(serviceBusCfg, context);

            (serviceBusCfg as IBusFactoryConfigurator).Received(1).ReceiveEndpoint(nameof(ConsumidorCriaOuAtualizaUsuario), Arg.Any<Action<IReceiveEndpointConfigurator>>());
            (serviceBusCfg as IBusFactoryConfigurator).Received(1).ReceiveEndpoint(nameof(ConsumidorDeletaUsuario), Arg.Any<Action<IReceiveEndpointConfigurator>>());
            (serviceBusCfg as IBusFactoryConfigurator).Received(1).ReceiveEndpoint(nameof(ConsumidorCriaOuAtualizaGrupoUsuario), Arg.Any<Action<IReceiveEndpointConfigurator>>());
            (serviceBusCfg as IBusFactoryConfigurator).Received(1).ReceiveEndpoint(nameof(ConsumidorDeletaGrupoUsuario), Arg.Any<Action<IReceiveEndpointConfigurator>>());
            (serviceBusCfg as IBusFactoryConfigurator).Received(5).ReceiveEndpoint(Arg.Any<string>(), Arg.Any<Action<IReceiveEndpointConfigurator>>());
        }

        [Fact]
        public void Configure_ServiceBus_WithNullContext_ShouldNotThrow()
        {
            var exception = Record.Exception(() => _configurator.Configure(_serviceBusCfg, null));
            Assert.Null(exception);
        }

        [Fact]
        public void Configure_ServiceBus_WithNullConfigurator_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => _configurator.Configure((IServiceBusBusFactoryConfigurator)null, _context));
        }

        #endregion

        #region AddReceiveEndPoint Method Tests (via endpoint configuration callback)

        [Fact]
        public void AddReceiveEndPoint_ShouldConfigureEndpointWithCorrectQueueName()
        {
            Action<IReceiveEndpointConfigurator> capturedEndpointAction = null;
            _rabbitMqCfg.When(x => x.ReceiveEndpoint(Arg.Any<string>(), Arg.Any<Action<IReceiveEndpointConfigurator>>()))
                       .Do(callInfo => capturedEndpointAction = callInfo.ArgAt<Action<IReceiveEndpointConfigurator>>(1));

            _configurator.Configure(_rabbitMqCfg, _context);

            (_rabbitMqCfg as IBusFactoryConfigurator).Received(1).ReceiveEndpoint(nameof(ConsumidorCriaOuAtualizaUsuario), Arg.Any<Action<IReceiveEndpointConfigurator>>());
        }

        [Fact]
        public void AddReceiveEndPoint_ShouldConfigureEndpointWithCustomQueueName()
        {
            _configurator.Configure(_rabbitMqCfg, _context);

            (_rabbitMqCfg as IBusFactoryConfigurator).Received(1).ReceiveEndpoint(Constants.cst_HealthCheck_Queue, Arg.Any<Action<IReceiveEndpointConfigurator>>());
        }

        [Fact]
        public void Configure_BothMethods_ShouldHaveSimilarBehavior()
        {
            _configurator.Configure(_rabbitMqCfg, _context);
            _configurator.Configure(_serviceBusCfg, _context);

            (_rabbitMqCfg as IBusFactoryConfigurator).Received(5).ReceiveEndpoint(Arg.Any<string>(), Arg.Any<Action<IReceiveEndpointConfigurator>>());
            (_serviceBusCfg as IBusFactoryConfigurator).Received(5).ReceiveEndpoint(Arg.Any<string>(), Arg.Any<Action<IReceiveEndpointConfigurator>>());
        }

        [Fact]
        public void Configure_MultipleCallsToSameMethod_ShouldNotCauseIssues()
        {
            _configurator.Configure(_rabbitMqCfg, _context);
            _configurator.Configure(_rabbitMqCfg, _context);

            (_rabbitMqCfg as IBusFactoryConfigurator).Received(10).ReceiveEndpoint(Arg.Any<string>(), Arg.Any<Action<IReceiveEndpointConfigurator>>());
        }

        [Fact]
        public void Configure_ServiceBusBusFactoryConfigurator_ConfiguresAllEndpoints()
        {
            var busFactoryConfigurator = Substitute.For<IServiceBusBusFactoryConfigurator>();
            var receiveConfigurator = (IReceiveConfigurator)busFactoryConfigurator;
            var busFactory = (IBusFactoryConfigurator)busFactoryConfigurator;

            var context = Substitute.For<IBusRegistrationContext>();
            var redisStatusNotifier = Substitute.For<IRedisStatusNotifier>();
            context.GetService<IRedisStatusNotifier>().Returns(redisStatusNotifier);

            var configurator = new ConfiguradorEndpoints();
            configurator.Configure(busFactoryConfigurator, context);

            receiveConfigurator.Received(1).ReceiveEndpoint(nameof(ConsumidorCriaOuAtualizaUsuario), Arg.Any<Action<IReceiveEndpointConfigurator>>());
            receiveConfigurator.Received(1).ReceiveEndpoint(nameof(ConsumidorDeletaUsuario), Arg.Any<Action<IReceiveEndpointConfigurator>>());
            receiveConfigurator.Received(1).ReceiveEndpoint(nameof(ConsumidorCriaOuAtualizaGrupoUsuario), Arg.Any<Action<IReceiveEndpointConfigurator>>());
            receiveConfigurator.Received(1).ReceiveEndpoint(nameof(ConsumidorDeletaGrupoUsuario), Arg.Any<Action<IReceiveEndpointConfigurator>>());
            receiveConfigurator.Received(1).ReceiveEndpoint(Constants.cst_HealthCheck_Queue, Arg.Any<Action<IReceiveEndpointConfigurator>>());

            busFactoryConfigurator.Received(1).ConnectConsumerConfigurationObserver(Arg.Any<ConcurrencyLimitConfigurationObserver>());
        }

        [Fact]
        public void Configure_IRabbitMqBusFactoryConfigurator_ConfiguresAllEndpoints()
        {
            var busFactoryConfigurator = Substitute.For<IRabbitMqBusFactoryConfigurator>();
            var receiveConfigurator = (IReceiveConfigurator)busFactoryConfigurator;
            var busFactory = (IBusFactoryConfigurator)busFactoryConfigurator;

            var context = Substitute.For<IBusRegistrationContext>();
            var redisStatusNotifier = Substitute.For<IRedisStatusNotifier>();
            context.GetService<IRedisStatusNotifier>().Returns(redisStatusNotifier);

            var configurator = new ConfiguradorEndpoints();
            configurator.Configure(busFactoryConfigurator, context);

            receiveConfigurator.Received(1).ReceiveEndpoint(nameof(ConsumidorCriaOuAtualizaUsuario), Arg.Any<Action<IReceiveEndpointConfigurator>>());
            receiveConfigurator.Received(1).ReceiveEndpoint(nameof(ConsumidorDeletaUsuario), Arg.Any<Action<IReceiveEndpointConfigurator>>());
            receiveConfigurator.Received(1).ReceiveEndpoint(nameof(ConsumidorCriaOuAtualizaGrupoUsuario), Arg.Any<Action<IReceiveEndpointConfigurator>>());
            receiveConfigurator.Received(1).ReceiveEndpoint(nameof(ConsumidorDeletaGrupoUsuario), Arg.Any<Action<IReceiveEndpointConfigurator>>());
            receiveConfigurator.Received(1).ReceiveEndpoint(Constants.cst_HealthCheck_Queue, Arg.Any<Action<IReceiveEndpointConfigurator>>());

            busFactoryConfigurator.Received(1).ConnectConsumerConfigurationObserver(Arg.Any<ConcurrencyLimitConfigurationObserver>());
        }
        #endregion

        #region Interface Implementation Tests

        [Fact]
        public void ReceiveEndpointsConfigurator_ShouldImplementIReceiveEndpointsConfigurator()
        {
            Assert.IsAssignableFrom<IConfiguradorEndpoints>(_configurator);
        }

        [Fact]
        public void EmptyReceiveEndpointsConfigurator_Configure_RabbitMq_DoesNotThrow()
        {
            var configurator = new ConfiguradorEndpointsVazio();
            var busFactoryConfigurator = Substitute.For<IRabbitMqBusFactoryConfigurator>();
            var context = Substitute.For<IBusRegistrationContext>();

            var exception = Record.Exception(() => configurator.Configure(busFactoryConfigurator, null));
            Assert.Null(exception);
        }

        [Fact]
        public void EmptyReceiveEndpointsConfigurator_Configure_ServiceBus_DoesNotThrow()
        {
            var configurator = new ConfiguradorEndpointsVazio();
            var busFactoryConfigurator = Substitute.For<IServiceBusBusFactoryConfigurator>();
            var context = Substitute.For<IBusRegistrationContext>();

            var exception = Record.Exception(() => configurator.Configure(busFactoryConfigurator, null));
            Assert.Null(exception);
        }

        [Fact]
        public void Empty_StaticProperty_ReturnsInstance()
        {
            var instance = Empty;

            Assert.NotNull(instance);
            Assert.IsAssignableFrom<IConfiguradorEndpoints>(instance);
        }

        #endregion
    }
}