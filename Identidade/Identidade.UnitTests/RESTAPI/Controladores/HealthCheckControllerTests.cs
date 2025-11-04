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

namespace Identidade.UnitTests.RESTAPI.Controllers
{
    public class HealthCheckControllerTests
    {
        [Fact]
        public void RespondPing_ReturnsOkResult()
        {
            var controller = new HealthCheckController(Substitute.For<IHealthCheckService>());
            var result = controller.RespondPing();

            var okResult = Assert.IsType<OkResult>(result);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
        }

        [Fact]
        public void ExecuteHealthCheck_Test()
        {
            var values = new HealthCheckValues(
                processId: "TestProcessId",
                processName: "TestProcessName",
                configItems: new Dictionary<string, string>() { { "TestConfigName", "TestConfigValue" } });

            var healthCheckService = Substitute.For<IHealthCheckService>();

            healthCheckService
                .Execute()
                .Returns(values);

            var controller = new HealthCheckController(healthCheckService);
            var result = controller.ExecuteHealthCheck();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);

            var actual = okResult.Value as HealthCheckValuesDto;
            Assert.NotNull(actual);
            Assert.Equal(JsonConvert.SerializeObject(values.ToDto()), JsonConvert.SerializeObject(actual));

            healthCheckService.Received(1).Execute();
        }
    }
}
