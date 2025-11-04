using MassTransit;
using MassTransit.Serialization;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Identidade.Publico.Enumerations;
using System;
using System.Collections.Generic;
using Xunit;
using Identidade.Infraestrutura.Configuracoes.ServiceBus;
using Identidade.Infraestrutura.Configuracoes;

namespace Identidade.UnitTests.Infraestrutura.Configurations.ServiceBus
{
    public class AzureServiceBusConfiguratorTests
    {
        private readonly ConfiguradorAzureServiceBus _configurator;
        private readonly IBusRegistrationConfigurator _busRegistrationConfigurator;
        private readonly ISettings _settings;
        private readonly IConfiguradorEndpoints _receiveEndpointsConfigurator;

        public AzureServiceBusConfiguratorTests()
        {
            _configurator = new ConfiguradorAzureServiceBus();
            _busRegistrationConfigurator = Substitute.For<IBusRegistrationConfigurator>();
            _receiveEndpointsConfigurator = Substitute.For<IConfiguradorEndpoints>();
            _settings = CreateTestSettings();
        }

        private static ConsumerSettings CreateTestSettings()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=TestDb;Trusted_Connection=true;",
                    ["AzureServiceBus:ConnectionString"] = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test",
                    ["AzureServiceBus:QueueName"] = "TestQueue",
                    ["MessageBroker"] = "AzureServiceBus",
                    ["AuthenticationType"] = "User",
                    ["Logging:WriteToFile"] = "true",
                    ["Logging:FilePath"] = "log\\test.log",
                    ["Logging:WriteToElasticSearch"] = "false",
                    ["Logging:ElasticSearchAddress"] = "http://localhost:9200",
                    ["Logging:LogLevel"] = "Information",
                    ["Redis.Url"] = "localhost,syncTimeout=15000,allowAdmin=true",
                    ["SharedCache.Redis.DefaultExpire"] = "1.00:00:00",
                    ["Redis.SetAliveInterval"] = "00:00:10",
                    ["CryptoKey"] = "test-crypto-key"
                })
                .Build();

            return new ConsumerSettings(configuration);
        }

        [Fact]
        public void Settings_ShouldHaveCorrectAzureServiceBusSettings()
        {
            Assert.Equal("Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test", _settings.AzureServiceBus.ConnectionString);
            Assert.Equal("TestQueue", _settings.AzureServiceBus.QueueName);
        }

        [Fact]
        public void Settings_ShouldHaveCorrectMessageBrokerType()
        {
            Assert.Equal(MessageBroker.AzureServiceBus, _settings.MessageBroker);
        }

        [Fact]
        public void Settings_ShouldHaveCorrectAuthenticationType()
        {
            Assert.Equal(SettingsAuthenticationType.User, _settings.AuthenticationType);
        }

        [Fact]
        public void Settings_ShouldHaveCorrectConnectionString()
        {
            Assert.NotNull(_settings.ConnectionStrings.DefaultConnection);
            Assert.Contains("TestDb", _settings.ConnectionStrings.DefaultConnection);
        }

        [Fact]
        public void Settings_ShouldHaveCorrectLoggingConfiguration()
        {
            Assert.True(_settings.Logging.WriteToFile);
            Assert.Equal("log\\test.log", _settings.Logging.FilePath);
            Assert.False(_settings.Logging.WriteToElasticSearch);
        }

        [Fact]
        public void AzureServiceBusConfigurator_ShouldImplementIMessageBrokerConfigurator()
        {
            Assert.IsAssignableFrom<IConfiguradorMessageBroker>(_configurator);
        }

        [Fact]
        public void AzureServiceBusConfigurator_ShouldBePublic()
        {
            var type = typeof(ConfiguradorAzureServiceBus);
            Assert.True(type.IsPublic);
        }

        [Fact]
        public void ConsumerSettings_ShouldImplementISettings()
        {
            Assert.IsAssignableFrom<ISettings>(_settings);
        }

        [Fact]
        public void ConfigureAzureServiceBus_SetsCorrectly()
        {
            var cfg = Substitute.For<IServiceBusBusFactoryConfigurator>();
            var context = Substitute.For<IBusRegistrationContext>();
            var settings = CreateTestSettings();
            var receiveEndpointsConfigurator = Substitute.For<IConfiguradorEndpoints>();

            ConfiguradorAzureServiceBus.ConfigureAzureServiceBus(cfg, context, settings, receiveEndpointsConfigurator);

            cfg.Received().AddSerializer(Arg.Any<NewtonsoftJsonSerializerFactory>());
            cfg.Received().AddDeserializer(Arg.Any<NewtonsoftJsonSerializerFactory>(), true);
            receiveEndpointsConfigurator.Received().Configure(cfg, context);
            cfg.Received().ConfigureEndpoints(context);
        }
    }
}