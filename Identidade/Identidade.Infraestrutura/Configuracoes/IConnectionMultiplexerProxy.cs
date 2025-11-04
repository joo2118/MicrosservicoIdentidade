using StackExchange.Redis;
using System;

namespace Identidade.Infraestrutura.Configuracoes
{
    public interface IConnectionMultiplexerProxy
    {
        void SetKey(RedisKey key, HashEntry[] hashFields, CommandFlags flags = CommandFlags.None);

        bool SetKeyExpiration(RedisKey key, TimeSpan? expiry = null, CommandFlags flags = CommandFlags.None);

        bool DeleteKey(RedisKey key, CommandFlags flags = CommandFlags.None);

        long GetRedisTime();
    }
}
