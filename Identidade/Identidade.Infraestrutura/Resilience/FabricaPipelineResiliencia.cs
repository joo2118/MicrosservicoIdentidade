using System;
using System.Threading.RateLimiting;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace Identidade.Infraestrutura.Resilience;

public static class FabricaPipelineResiliencia
{
    public static ResiliencePipeline Create(OpcoesResiliencia options = null)
    {
        options ??= new OpcoesResiliencia();

        var builder = new ResiliencePipelineBuilder()
            .AddConcurrencyLimiter(new ConcurrencyLimiterOptions
            {
                PermitLimit = options.ConcorrenciaMaxima,
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            })
            .AddTimeout(options.Timeout)
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = options.NumeroRetry,
                Delay = options.DelayRetry,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder()
                    .Handle<TimeoutRejectedException>()
                    .Handle<ExceptionTransitoriaSQL>()
                    .Handle<InvalidOperationException>()
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 1.0,
                MinimumThroughput = options.LimiteFalhasCircuitBreaker,
                SamplingDuration = TimeSpan.FromSeconds(30),
                BreakDuration = options.DuracaoAberturaCircuitBreaker,
                ShouldHandle = new PredicateBuilder()
                    .Handle<TimeoutRejectedException>()
                    .Handle<ExceptionTransitoriaSQL>()
                    .Handle<InvalidOperationException>()
            });

        return builder.Build();
    }

    /// <summary>
    /// Pipeline específico para publicação no barramento (MassTransit).
    /// Mantém apenas timeout + retry (sem concorrência/circuit breaker) para não bloquear nem abrir circuito por falhas externas.
    /// </summary>
    public static ResiliencePipeline CreateBusPublish(OpcoesResiliencia options = null)
    {
        options ??= new OpcoesResiliencia();

        return new ResiliencePipelineBuilder()
            .AddTimeout(options.Timeout)
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = options.NumeroRetry,
                Delay = options.DelayRetry,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder()
                    .Handle<TimeoutRejectedException>()
                    .Handle<ExceptionTransitoriaSQL>()
                    .Handle<InvalidOperationException>()
            })
            .Build();
    }
}
