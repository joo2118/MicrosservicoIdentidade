using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Identidade.Consumidor.Filters
{
    public class BaselineMetricsFilter<T> : IFilter<ConsumeContext<T>> where T : class
    {
        private readonly ILogger<BaselineMetricsFilter<T>> _logger;

        public BaselineMetricsFilter(ILogger<BaselineMetricsFilter<T>> logger)
        {
            _logger = logger;
        }

        public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
        {
            var stopwatch = Stopwatch.StartNew();
            var messageType = typeof(T).Name;
            var messageId = context.MessageId?.ToString() ?? "unknown";
            Exception consumeException = null;

            try
            {
                await next.Send(context);
            }
            catch (Exception ex)
            {
                consumeException = ex;
                throw;
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogInformation(
                    "[BASELINE_METRIC] Message {MessageType} (ID: {MessageId}) processed in {ElapsedMs}ms. Success: {Success}. Exception: {Exception}",
                    messageType,
                    messageId,
                    stopwatch.ElapsedMilliseconds,
                    consumeException == null,
                    consumeException?.Message ?? "None"
                );
            }
        }

        public void Probe(ProbeContext context)
        {
            context.CreateFilterScope("baselineMetrics");
        }
    }
}