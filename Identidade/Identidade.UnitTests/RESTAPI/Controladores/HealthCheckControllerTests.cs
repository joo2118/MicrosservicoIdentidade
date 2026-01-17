using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NSubstitute;
using Identidade.Infraestrutura.Entidades;
using Identidade.Infraestrutura.Interfaces;
using Identidade.Publico.Dtos;
using Identidade.RESTAPI.Controllers;
using System.Collections.Generic;
using System.Net;
using Xunit;
using Identidade.Infraestrutura.Extensoes;
using Serilog;
using System.Threading.Tasks;

namespace Identidade.UnitTests.RESTAPI.Controllers
{
    public class HealthCheckControllerTests
    {
        private static TelemetryClient CreateTelemetryClient()
        {
            var telemetryConfiguration = new TelemetryConfiguration
            {
                TelemetryChannel = new InMemoryChannel()
            };

            return new TelemetryClient(telemetryConfiguration);
        }

        [Fact]
        public async Task RespondPing_ReturnsOkResult()
        {
            var controller = new HealthCheckController(
                Substitute.For<IHealthCheckService>(),
                CreateTelemetryClient(),
                Substitute.For<ILogger>());

            var result = await controller.RespondPing();

            var okResult = Assert.IsType<OkResult>(result);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
        }

        [Fact]
        public async Task ExecuteHealthCheck_Test()
        {
            var values = new HealthCheckValues(
                processId: "TestProcessId",
                processName: "TestProcessName",
                configItems: new Dictionary<string, string> { { "TestConfigName", "TestConfigValue" } });

            var healthCheckService = Substitute.For<IHealthCheckService>();
            healthCheckService.Execute().Returns(values);

            var controller = new HealthCheckController(
                healthCheckService,
                CreateTelemetryClient(),
                Substitute.For<ILogger>());

            var result = await controller.ExecuteHealthCheck();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);

            var actual = Assert.IsType<HealthCheckValuesDto>(okResult.Value);
            Assert.Equal(JsonConvert.SerializeObject(values.ToDto()), JsonConvert.SerializeObject(actual));

            healthCheckService.Received(1).Execute();
        }
    }
}
