using Identidade.Publico.Dtos;
using System;
using System.Collections.Generic;
using Xunit;

namespace Identidade.Publico.Events
{
    public class HealthCheckIdentityEventTests
    {
        [Theory]
        [InlineData("TestProcessName", null)]
        [InlineData(null, "TestErrorMessage")]
        public void Constructor_Test(string processName, string errorMessage)
        {
            var id = Guid.NewGuid();

            var values = !string.IsNullOrWhiteSpace(processName)
                ? new HealthCheckValuesDto(
                    processId: "TestProcessId",
                    processName: processName,
                    configItems: new Dictionary<string, string>() { { "TestConfigName", "TestConfigValue" } })
                : null;

            var actual = new HealthCheckIdentityEvent(id, values, errorMessage);

            Assert.Equal(id, actual.Id);
            Assert.Equal(values, actual.Values);
            Assert.Equal(errorMessage, actual.ErrorMessage);
        }

        [Fact]
        public void Constructor_NeitherValuesOrError_Test()
        {
            var id = Guid.NewGuid();

            var ex = Assert.Throws<ArgumentException>(() => new HealthCheckIdentityEvent(id, null, null));

            Assert.NotNull(ex);
        }

        [Fact]
        public void Constructor_BothValuesAndError_Test()
        {
            var id = Guid.NewGuid();

            var values = new HealthCheckValuesDto(
                processId: "TestProcessId",
                processName: "TestProcessName",
                configItems: new Dictionary<string, string>() { { "TestConfigName", "TestConfigValue" } });

            var errorMessage = "TestErrorMessage";

            var ex = Assert.Throws<ArgumentException>(() => new HealthCheckIdentityEvent(id, values, errorMessage));

            Assert.NotNull(ex);
        }

        [Fact]
        public void FromValues_Test()
        {
            var id = Guid.NewGuid();

            var values = new HealthCheckValuesDto(
                processId: "TestProcessId",
                processName: "TestProcessName",
                configItems: new Dictionary<string, string>() { { "TestConfigName", "TestConfigValue" } });

            var actual = HealthCheckIdentityEvent.FromValues(id, values);

            Assert.Equal(id, actual.Id);
            Assert.Equal(values, actual.Values);
            Assert.Null(actual.ErrorMessage);
        }

        [Fact]
        public void FromErrorMessage_Test()
        {
            var id = Guid.NewGuid();

            var errorMessage = "TestErrorMessage";

            var actual = HealthCheckIdentityEvent.FromErrorMessage(id, errorMessage);

            Assert.Equal(id, actual.Id);
            Assert.Null(actual.Values);
            Assert.Equal(errorMessage, actual.ErrorMessage);
        }
    }
}
