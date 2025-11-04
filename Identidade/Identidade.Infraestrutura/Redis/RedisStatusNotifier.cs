using Identidade.Dominio.Interfaces;
using Identidade.Infraestrutura.Configuracoes;
using Serilog;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Timers;

namespace Identidade.Infraestrutura.RedisNotifier
{
    public class RedisStatusNotifier : IRedisStatusNotifier
    {
        private readonly Timer _timer;
        private readonly IConnectionMultiplexerProxy _connection;
        private readonly string _applicationIdentifier;
        private readonly string _applicationId;

        public RedisStatusNotifier(Timer timer, IConnectionMultiplexerProxy connection, string applicationIdentifier, string applicationId)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _timer = timer ?? throw new ArgumentNullException(nameof(timer));
            _timer.Elapsed += (sender, args) => SetLastAlive();
            _applicationIdentifier = !string.IsNullOrWhiteSpace(applicationIdentifier) ? applicationIdentifier : throw new ArgumentException($"{nameof(applicationIdentifier)} cannot be null, empty or white-space.", paramName: nameof(applicationIdentifier));
            _applicationId = !string.IsNullOrWhiteSpace(applicationId) ? applicationId : throw new ArgumentException($"{nameof(applicationId)} cannot be null, empty or white-space.", paramName: nameof(applicationId));
        }

        public IDisposable SetWorking(string strMsgId)
        {
            var redisTime = _connection.GetRedisTime();
            string processId = GetConsumerId();

            var msgKey = RedisGetMessageKey(strMsgId);

            _connection.SetKey(msgKey, new[] {
                new HashEntry(RedisConstants.Field.REDIS_FIELD_STATUS, RedisConstants.Status.REDIS_STATUS_WORKING),
                new HashEntry(RedisConstants.Field.REDIS_FIELD_STATUS_TIME, redisTime),
                new HashEntry(_applicationId, processId)
            });

            _connection.SetKeyExpiration(msgKey);

            return Disposable.Create(() => _connection.DeleteKey(msgKey));
        }

        public void SetIdle()
        {
            var redisTime = _connection.GetRedisTime();
            SetWorkerKeys(
                    new HashEntry(RedisConstants.Field.REDIS_FIELD_STATUS, RedisConstants.Status.REDIS_STATUS_IDLE),
                    new HashEntry(RedisConstants.Field.REDIS_FIELD_STATUS_TIME, redisTime));
        }

        public void SetStarting()
        {
            Log.Information("Service Starting with process id {ProcessId}");

            var redisTime = _connection.GetRedisTime();
            SetWorkerKeys(
                new HashEntry(RedisConstants.Field.REDIS_FIELD_STATUS, RedisConstants.Status.REDIS_STATUS_STARTING),
                new HashEntry(RedisConstants.Field.REDIS_FIELD_STATUS_TIME, redisTime));

            _timer.Start();
        }

        private void SetLastAlive()
        {
            var redisTime = _connection.GetRedisTime();

            SetWorkerKeys(new HashEntry(RedisConstants.Field.REDIS_FIELD_LASTALIVE, redisTime));

            _connection.SetKeyExpiration(GetConsumerId(), null, CommandFlags.FireAndForget);
        }

        private void SetWorkerKeys(params HashEntry[] keys)
        {
            string consumerId = GetConsumerId();

            var entries = new List<HashEntry>
            {
                new(RedisConstants.Field.REDIS_FIELD_MACHINE, Environment.MachineName)
            };
            entries.AddRange(keys);

            var key = RedisGetConsumerKey(consumerId);
            _connection.SetKey(key, entries.ToArray());
            _connection.SetKeyExpiration(key);
        }

        private static string GetConsumerId() =>
            string.Concat(Environment.ProcessId, '@', Environment.MachineName);

        private RedisKey RedisGetConsumerKey(string consumerId) =>
            string.Concat(_applicationIdentifier, consumerId);

        private static RedisKey RedisGetMessageKey(string messageId) =>
            string.Concat(RedisConstants.Path.REDIS_MESSAGE_ACTIVE, messageId);
    }
}
