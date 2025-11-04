using Microsoft.Extensions.Configuration;
using Identidade.RESTAPI.Configurations;
using System;
using System.Collections.Generic;
using Xunit;

namespace Identidade.UnitTests.RESTAPI.Configurations
{
    public class RestApiSettingsTests
    {
        [Fact]
        public void RestApiSettings_Constructor_InitializesProperties()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                {"HealthCheck:MaxMemory", "1024"},
                {"JWT:Issuer", "testIssuer"},
                {"JWT:Audience", "testAudience"},
                {"Redis.Url", "testRedisUrl"},
                {"SharedCache.Redis.DefaultExpire", "00:30:00"},
                {"Redis.SetAliveInterval", "00:00:30"}
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
            var settings = new RestApiSettings(configuration);

            Assert.NotNull(settings.HealthCheck);
            Assert.Equal(1024, settings.HealthCheck.MaxMemory);

            Assert.NotNull(settings.Jwt);
            Assert.Equal("testIssuer", settings.Jwt.Issuer);
            Assert.Equal("testAudience", settings.Jwt.Audience);

            Assert.Equal("testRedisUrl", settings.RedisUrl);
            Assert.Equal(TimeSpan.FromMinutes(30), settings.SharedCacheRedisDefaultExpire);
            Assert.Equal(TimeSpan.FromSeconds(30), settings.RedisSetAliveInterval);
        }

        [Fact]
        public void RestApiSettings_Constructor_DefaultValues()
        {
            var inMemorySettings = new Dictionary<string, string> { };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
            var settings = new RestApiSettings(configuration);

            Assert.NotNull(settings.HealthCheck);
            Assert.Equal(0, settings.HealthCheck.MaxMemory);

            Assert.NotNull(settings.Jwt);
            Assert.Empty(settings.Jwt.Issuer);
            Assert.Empty(settings.Jwt.Audience);

            Assert.Empty(settings.RedisUrl);
            Assert.Equal(TimeSpan.FromDays(1), settings.SharedCacheRedisDefaultExpire);
            Assert.Equal(TimeSpan.FromMilliseconds(10000), settings.RedisSetAliveInterval);
        }
    }
}

