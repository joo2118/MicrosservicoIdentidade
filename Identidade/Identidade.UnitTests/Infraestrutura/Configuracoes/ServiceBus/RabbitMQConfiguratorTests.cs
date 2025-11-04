using MassTransit;
using MassTransit.Serialization;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Identidade.Consumidor.Configuracoes;
using Identidade.Publico.Enumerations;
using System;
using System.Collections.Generic;
using Xunit;
using Identidade.Infraestrutura.Configuracoes.ServiceBus;
using Identidade.Infraestrutura.Configuracoes;

namespace Identidade.UnitTests.Infraestrutura.Configurations.ServiceBus
{
    public class RabbitMQConfiguratorTests
    {
        private readonly ConfiguradorRabbitMq _configurator;
        private readonly IBusRegistrationConfigurator _busRegistrationConfigurator;
        private readonly IConfiguradorEndpoints _receiveEndpointsConfigurator;
        private readonly ConsumerSettings _settings;

        public RabbitMQConfiguratorTests()
        {
            _configurator = new ConfiguradorRabbitMq();
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
                    ["RabbitMQ:Host"] = "localhost",
                    ["RabbitMQ:VirtualHost"] = "/",
                    ["RabbitMQ:Username"] = "guest",
                    ["RabbitMQ:Password"] = "guest",
                    ["AzureServiceBus:ConnectionString"] = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test",
                    ["AzureServiceBus:QueueName"] = "TestQueue",
                    ["MessageBroker"] = "RabbitMQ",
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

        private static ConsumerSettings CreateCustomRabbitMqSettings(string host, string virtualHost, string username, string password)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=TestDb;Trusted_Connection=true;",
                    ["RabbitMQ:Host"] = host,
                    ["RabbitMQ:VirtualHost"] = virtualHost,
                    ["RabbitMQ:Username"] = username,
                    ["RabbitMQ:Password"] = password,
                    ["AzureServiceBus:ConnectionString"] = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test",
                    ["AzureServiceBus:QueueName"] = "TestQueue",
                    ["MessageBroker"] = "RabbitMQ",
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
        public void Configure_WithDifferentRabbitMqSettings_ShouldUseCorrectValues()
        {
            var customSettings = CreateCustomRabbitMqSettings("custom-host", "/custom", "custom-user", "custom-pass");

            _configurator.Configure(_busRegistrationConfigurator, customSettings, _receiveEndpointsConfigurator);

            Assert.Equal("custom-host", customSettings.RabbitMQ.Host);
            Assert.Equal("/custom", customSettings.RabbitMQ.VirtualHost);
            Assert.Equal("custom-user", customSettings.RabbitMQ.UserName);
            Assert.Equal("custom-pass", customSettings.RabbitMQ.Password);
        }

        [Fact]
        public void Settings_ShouldHaveCorrectMessageBrokerType()
        {
            Assert.Equal(MessageBroker.RabbitMQ, _settings.MessageBroker);
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
        public void Settings_ShouldHaveCorrectRedisConfiguration()
        {
            Assert.Equal("localhost,syncTimeout=15000,allowAdmin=true", _settings.RedisUrl);
            Assert.Equal(TimeSpan.FromDays(1), _settings.SharedCacheRedisDefaultExpire);
            Assert.Equal(TimeSpan.FromSeconds(10), _settings.RedisSetAliveInterval);
        }

        [Fact]
        public void RabbitMQConfigurator_ShouldImplementIMessageBrokerConfigurator()
        {
            Assert.IsAssignableFrom<IConfiguradorMessageBroker>(_configurator);
        }

        [Fact]
        public void RabbitMQConfigurator_ShouldBePublic()
        {
            var type = typeof(ConfiguradorRabbitMq);
            Assert.True(type.IsPublic);
        }

        [Fact]
        public void ConsumerSettings_ShouldImplementISettings()
        {
            Assert.IsAssignableFrom<ISettings>(_settings);
        }

        [Fact]
        public void ConsumerSettings_ShouldImplementIRedisSettings()
        {
            Assert.IsAssignableFrom<IRedisSettings>(_settings);
        }

        [Fact]
        public void Settings_FromConfiguration_ShouldMatchExpectedValues()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["RabbitMQ:Host"] = "test-host",
                    ["RabbitMQ:VirtualHost"] = "/test",
                    ["RabbitMQ:Username"] = "test-user",
                    ["RabbitMQ:Password"] = "test-password"
                })
                .Build();

            var settings = new ConsumerSettings(configuration);

            Assert.Equal("test-host", settings.RabbitMQ.Host);
            Assert.Equal("/test", settings.RabbitMQ.VirtualHost);
            Assert.Equal("test-user", settings.RabbitMQ.UserName);
            Assert.Equal("test-password", settings.RabbitMQ.Password);
        }

        [Fact]
        public void ConfigureRabbitMq_SetsCorrectly()
        {
            var cfg = Substitute.For<IRabbitMqBusFactoryConfigurator>();
            var context = Substitute.For<IBusRegistrationContext>();
            var settings = CreateTestSettings();
            var receiveEndpointsConfigurator = Substitute.For<IConfiguradorEndpoints>();

            ConfiguradorRabbitMq.ConfigureRabbitMq(cfg, context, settings, receiveEndpointsConfigurator);

            cfg.Received().AddSerializer(Arg.Any<NewtonsoftJsonSerializerFactory>());
            cfg.Received().AddDeserializer(Arg.Any<NewtonsoftJsonSerializerFactory>(), true);
            receiveEndpointsConfigurator.Received().Configure(cfg, context);
            cfg.Received().ConfigureEndpoints(context);
        }
    }
}