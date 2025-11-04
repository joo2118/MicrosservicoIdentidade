using Microsoft.Extensions.Configuration;
using Identidade.Infraestrutura.Configuracoes;
using System;
using System.Collections.Generic;
using Xunit;

namespace Identidade.UnitTests.Infraestrutura.Configurations
{
    public class ConsumerSettingsTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            var inMemorySettings = new Dictionary<string, string>
                {
                    {"Redis.Url", "localhost"},
                    {"SharedCache.Redis.DefaultExpire", "2.00:00:00"},
                    {"Redis.SetAliveInterval", "00:00:20"}
                };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var settings = new ConsumerSettings(configuration);

            Assert.Equal("localhost", settings.RedisUrl);
            Assert.Equal(TimeSpan.FromDays(2), settings.SharedCacheRedisDefaultExpire);
            Assert.Equal(TimeSpan.FromSeconds(20), settings.RedisSetAliveInterval);
        }

        [Fact]
        public void Constructor_ShouldSetDefaultValues_WhenConfigurationValuesAreMissing()
        {
            var inMemorySettings = new Dictionary<string, string>();

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var settings = new ConsumerSettings(configuration);

            Assert.Equal(string.Empty, settings.RedisUrl);
            Assert.Equal(TimeSpan.FromDays(1), settings.SharedCacheRedisDefaultExpire);
            Assert.Equal(TimeSpan.FromMilliseconds(10000), settings.RedisSetAliveInterval);
        }
    }
}


