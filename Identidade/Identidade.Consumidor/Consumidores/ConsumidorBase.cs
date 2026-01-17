using MassTransit;
using Identidade.Consumidor.Helpers;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using System.Diagnostics;
using System;
using System.Collections.Generic;

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
                _telemetryClient.TrackException(ex, new Dictionary<string, string> 
                { 
                    { "MessageType", messageType },
                    { "MessageId", context.MessageId.ToString() }
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
    }
}
