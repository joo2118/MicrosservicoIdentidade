using Microsoft.Extensions.Configuration;
using System;
using Identidade.Infraestrutura.Configuracoes;

namespace Identidade.RESTAPI.Configurations
{
    /// <summary>
    /// Represents the settings for the REST API.
    /// </summary>
    public class RestApiSettings : SharedSettings, IRedisSettings
    {
        /// <summary>
        /// Gets the health check settings.
        /// </summary>
        public HealthCheck HealthCheck { get; }

        /// <summary>
        /// Gets the JWT settings.
        /// </summary>
        public Jwt Jwt { get; }

        /// <summary>
        /// Gets the Redis URL.
        /// </summary>
        public string RedisUrl { get; }

        /// <summary>
        /// Gets the default expiration time for the shared cache Redis.
        /// </summary>
        public TimeSpan SharedCacheRedisDefaultExpire { get; }

        /// <summary>
        /// Gets the interval for setting Redis alive.
        /// </summary>
        public TimeSpan RedisSetAliveInterval { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestApiSettings"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public RestApiSettings(IConfiguration configuration)
            : base(configuration)
        {
            HealthCheck = new HealthCheck
            (
                maxMemory: configuration.GetSection("HealthCheck").GetValue("MaxMemory", 0)
            );

            Jwt = new Jwt
            (
                issuer: configuration.GetValue("JWT:Issuer", string.Empty),
                audience: configuration.GetValue("JWT:Audience", string.Empty)
            );

            RedisUrl = configuration.GetValue("Redis.Url", string.Empty);
            SharedCacheRedisDefaultExpire = configuration.GetValue("SharedCache.Redis.DefaultExpire", TimeSpan.FromDays(1));
            RedisSetAliveInterval = configuration.GetValue("Redis.SetAliveInterval", TimeSpan.FromMilliseconds(10000));
        }
    }

    /// <summary>
    /// Represents the JWT settings.
    /// </summary>
    public class Jwt
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Jwt"/> class.
        /// </summary>
        /// <param name="issuer">The issuer.</param>
        /// <param name="audience">The audience.</param>
        public Jwt(string issuer, string audience)
        {
            Issuer = issuer;
            Audience = audience;
        }

        /// <summary>
        /// Gets the issuer.
        /// </summary>
        public string Issuer { get; }

        /// <summary>
        /// Gets the audience.
        /// </summary>
        public string Audience { get; }
    }

    /// <summary>
    /// Represents the health check settings.
    /// </summary>
    public class HealthCheck
    {
        /// <summary>
        /// Gets the maximum memory for the health check.
        /// </summary>
        public int MaxMemory { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthCheck"/> class.
        /// </summary>
        /// <param name="maxMemory">The maximum memory.</param>
        public HealthCheck(int maxMemory)
        {
            MaxMemory = maxMemory;
        }
    }
}
