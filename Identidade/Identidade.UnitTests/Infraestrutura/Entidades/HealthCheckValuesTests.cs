using Identidade.Infraestrutura.Entidades;
using System.Collections.Generic;
using Xunit;

namespace Identidade.UnitTests.Infraestrutura.Entities
{
    public class HealthCheckValuesTests
    {
        [Fact]
        public void Constructor_Test()
        {
            var processId = "TestProcessId";
            var processName = "TestProcessName";
            var configItems = new Dictionary<string, string>() { { "TestConfigName", "TestConfigValue" } };

            var healthCheckValues = new HealthCheckValues(processId, processName, configItems);

            Assert.Equal(processId, healthCheckValues.ProcessId);
            Assert.Equal(processName, healthCheckValues.ProcessName);
            Assert.Equal(configItems, healthCheckValues.ConfigItems);
        }
    }
}
