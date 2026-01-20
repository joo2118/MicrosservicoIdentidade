using MassTransit;
using Identidade.Consumidor.Helpers;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using Identidade.Publico.Eventos;

namespace Identidade.Consumidor.Consumidores
{
    public abstract class ConsumidorBase<T> : IConsumer<T>
        where T : class
    {
        private static object _lockKey = new object();

        private readonly IMessageManager _massageManager;
        protected readonly TelemetryClient _telemetryClient;

        public ConsumidorBase(IMessageManager massageManager, TelemetryClient telemetryClient)
        {
            _massageManager = massageManager;
            _telemetryClient = telemetryClient;
        }

        public async Task Consume(ConsumeContext<T> context)
        {
            var stopwatch = Stopwatch.StartNew();
            var messageType = typeof(T).Name;

            try
            {
                lock (_lockKey)
                {
                    if (_massageManager.VerifyMessageAlreadyConsumed(context.MessageId))
                    {
                        _telemetryClient.TrackEvent("MessageAlreadyConsumed", new Dictionary<string, string>
                        {
                            { "MessageType", messageType },
                            { "MessageId", context.MessageId.ToString() }
                        });
                        return;
                    }

                    _massageManager.SaveMessageId(context.MessageId).Wait();
                }

                await ConsumeContext(context);

                _telemetryClient.TrackEvent("MessageConsumed", new Dictionary<string, string>
                {
                    { "MessageType", messageType },
                    { "MessageId", context.MessageId.ToString() }
                });
            }
            catch (Exception ex)
            {
                var action = GetCommandName() ?? GetType().Name;

                var errorEvent = new ErrorEvent
                {
                    Consumer = GetType().Name,
                    Action = action,
                    MessageType = messageType,
                    MessageId = context.MessageId?.ToString(),
                    CorrelationId = context.CorrelationId?.ToString(),
                    ConversationId = context.ConversationId?.ToString(),
                    ExceptionType = ex.GetType().FullName,
                    ExceptionMessage = ex.Message,
                    StackTrace = ex.StackTrace
                };

                foreach (var kv in GetErrorMetadata(context) ?? new Dictionary<string, string>())
                    errorEvent.Metadata[kv.Key] = kv.Value;

                try
                {
                    await context.Publish(errorEvent);
                }
                catch (Exception publishEx)
                {
                    _telemetryClient.TrackException(publishEx, new Dictionary<string, string>
                    {
                        { "MessageType", messageType },
                        { "MessageId", context.MessageId?.ToString() },
                        { "Publish", nameof(ErrorEvent) }
                    });
                }

                _telemetryClient.TrackException(ex, new Dictionary<string, string>
                {
                    { "MessageType", messageType },
                    { "MessageId", context.MessageId.ToString() },
                    { "Action", action }
                });

                throw;
            }
            finally
            {
                stopwatch.Stop();
                _telemetryClient.TrackMetric($"{messageType}_ConsumeDuration", stopwatch.ElapsedMilliseconds);
            }
        }

        public abstract Task ConsumeContext(ConsumeContext<T> context);

        protected virtual string GetCommandName() => null;

        protected virtual Dictionary<string, string> GetErrorMetadata(ConsumeContext<T> context) => null;
    }
}
