using MassTransit;
using NSubstitute;
using Xunit;
using Identidade.Infraestrutura.Configuracoes.ServiceBus;
using Identidade.Infraestrutura.Configuracoes;

namespace Identidade.UnitTests.Infraestrutura.Configurations.ServiceBus
{
    public class MessageBrokerConfiguratorTests
    {
        [Fact]
        public void EmptyMessageBrokerConfigurator_Configure_DoesNotThrow()
        {
            var configurator = new ConfiguradorMessageBroker.ConfiguradorMessageBrokerVazio();
            var busRegistrationConfigurator = Substitute.For<IBusRegistrationConfigurator>();
            var settings = Substitute.For<ISettings>();
            var receiveEndpointsConfigurator = Substitute.For<IConfiguradorEndpoints>();

            var exception = Record.Exception(() =>
                configurator.Configure(busRegistrationConfigurator, settings, receiveEndpointsConfigurator)
            );
            Assert.Null(exception);
        }

        [Fact]
        public void Empty_StaticProperty_ReturnsInstance()
        {
            var instance = ConfiguradorMessageBroker.Empty;

            Assert.NotNull(instance);
            Assert.IsAssignableFrom<IConfiguradorMessageBroker>(instance);
        }
    }
}