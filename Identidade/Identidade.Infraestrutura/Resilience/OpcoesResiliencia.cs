using System;

namespace Identidade.Infraestrutura.Resilience;

public sealed class OpcoesResiliencia
{
    /// <summary>
    /// Tempo limite padrão aplicado às operações de serviço/repositório.
    /// </summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Número de tentativas (retries) para falhas transitórias.
    /// </summary>
    public int NumeroRetry { get; init; } = 2;

    /// <summary>
    /// Atraso base usado para backoff exponencial.
    /// </summary>
    public TimeSpan DelayRetry { get; init; } = TimeSpan.FromMilliseconds(200);

    /// <summary>
    /// Máximo de execuções concorrentes permitido.
    /// </summary>
    public int ConcorrenciaMaxima { get; init; } = 50;

    /// <summary>
    /// Circuit breaker: quantidade de falhas consecutivas antes de abrir.
    /// </summary>
    public int LimiteFalhasCircuitBreaker { get; init; } = 10;

    /// <summary>
    /// Circuit breaker: tempo para manter o circuito aberto.
    /// </summary>
    public TimeSpan DuracaoAberturaCircuitBreaker { get; init; } = TimeSpan.FromSeconds(30);
}
