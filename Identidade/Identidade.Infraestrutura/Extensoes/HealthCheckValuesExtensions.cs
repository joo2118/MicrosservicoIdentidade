using Identidade.Infraestrutura.Entidades;
using Identidade.Publico.Dtos;

namespace Identidade.Infraestrutura.Extensoes
{
    public static class HealthCheckValuesExtensions
    {
        public static HealthCheckValuesDto ToDto(this HealthCheckValues values)
        {
            return new HealthCheckValuesDto(values.ProcessId, values.ProcessName, values.ConfigItems);
        }
    }
}
