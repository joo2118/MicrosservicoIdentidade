using Identidade.Infraestrutura.Entidades;

namespace Identidade.Infraestrutura.Interfaces
{
    public interface IHealthCheckService
    {
        HealthCheckValues Execute();
    }
}
