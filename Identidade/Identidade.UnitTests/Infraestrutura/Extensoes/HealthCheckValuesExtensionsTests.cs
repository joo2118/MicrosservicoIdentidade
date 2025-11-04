using Identidade.Infraestrutura.Entidades;
using Identidade.Infraestrutura.Extensoes;
using System.Collections.Generic;
using Xunit;

namespace Identidade.UnitTests.Infraestrutura.Extensions
{
    public class HealthCheckValuesExtensionsTests
    {
        [Fact]
        public void ToDto_Test()
        {
            var values = new HealthCheckValues(
                processId: "TestProcessId",
                processName: "TestProcessName",
                configItems: new Dictionary<string, string>() { { "TestConfigName", "TestConfigValue" } });

            var actual = values.ToDto();

            Assert.Equal(values.ProcessId, actual.ProcessId);
            Assert.Equal(values.ProcessName, actual.ProcessName);
            Assert.Equal(values.ConfigItems, actual.ConfigItems);
        }
    }
}
