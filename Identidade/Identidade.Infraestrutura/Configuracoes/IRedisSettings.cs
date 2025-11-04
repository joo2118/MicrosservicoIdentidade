using System;

namespace Identidade.Infraestrutura.Configuracoes
{
    public interface IRedisSettings : ISettings
    {
        string RedisUrl { get; }
        TimeSpan SharedCacheRedisDefaultExpire { get; }
        TimeSpan RedisSetAliveInterval { get; }
    }
}
