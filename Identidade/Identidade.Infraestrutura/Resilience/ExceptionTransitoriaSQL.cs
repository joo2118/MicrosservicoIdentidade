using System;

namespace Identidade.Infraestrutura.Resilience;

/// <summary>
/// Wrapper para marcar erros de SQL transitórios.
/// </summary>
public sealed class ExceptionTransitoriaSQL : Exception
{
    public ExceptionTransitoriaSQL(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
