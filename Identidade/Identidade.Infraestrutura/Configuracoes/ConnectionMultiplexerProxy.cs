using StackExchange.Redis;
using System;

namespace Identidade.Infraestrutura.Configuracoes
{
    public class ConnectionMultiplexerProxy : IConnectionMultiplexerProxy
    {
        private readonly IRedisSettings _settings;
        private readonly ConnectionMultiplexer _redisConnection;

        public ConnectionMultiplexerProxy(IRedisSettings settings, string redisUrl)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            if (string.IsNullOrWhiteSpace(redisUrl))
                throw new ArgumentException("RedisUrl cannot be null, empty or white-space.", nameof(redisUrl));

            _redisConnection = ConnectionMultiplexer.Connect(redisUrl);
        }

        public void SetKey(RedisKey key, HashEntry[] hashFields, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase();
            db.HashSet(key, hashFields);
        }

        public bool SetKeyExpiration(RedisKey key, TimeSpan? expiry = null, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase();
            return db.KeyExpire(key, expiry ?? _settings.SharedCacheRedisDefaultExpire, flags);
        }

        public bool DeleteKey(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase();
            return db.KeyDelete(key);
        }

        public long GetRedisTime() =>
            (long)GetDatabase().ScriptEvaluate("return redis.call('TIME')[1]");

        private IDatabase GetDatabase() => _redisConnection.GetDatabase();
    }
}
