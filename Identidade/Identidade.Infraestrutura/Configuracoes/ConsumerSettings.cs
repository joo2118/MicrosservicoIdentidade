using Microsoft.Extensions.Configuration;
using System;

namespace Identidade.Infraestrutura.Configuracoes
{
    public class ConsumerSettings : SharedSettings, IRedisSettings
    {
        public string RedisUrl { get; }
        public TimeSpan SharedCacheRedisDefaultExpire { get; }
        public TimeSpan RedisSetAliveInterval { get; }

        public ConsumerSettings(IConfiguration configuration)
            : base(configuration)
        {
            RedisUrl = configuration.GetValue("Redis.Url", string.Empty);
            SharedCacheRedisDefaultExpire = configuration.GetValue("SharedCache.Redis.DefaultExpire", TimeSpan.FromDays(1));
            RedisSetAliveInterval = configuration.GetValue("Redis.SetAliveInterval", TimeSpan.FromMilliseconds(10000));
        }
    }
}
