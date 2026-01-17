using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Identidade.Infraestrutura.Interfaces;
using Identidade.Publico.Dtos;
using System;
using Identidade.Infraestrutura.Extensoes;
using Microsoft.ApplicationInsights;
using Serilog;
using System.Threading.Tasks;
using Identidade.RESTAPI.Controladores;

namespace Identidade.RESTAPI.Controllers
{
    /// <summary>
    /// Controller for HealthCheck purpose
    /// </summary>
    [Route("healthcheck")]
    public class HealthCheckController : BaseController
    {
        private readonly IHealthCheckService _healthCheckService;

        /// <summary>
        /// Constructor.
        /// </summary>
        public HealthCheckController(IHealthCheckService healthCheckService, TelemetryClient telemetryClient, ILogger logger)
            : base(telemetryClient, logger)
        {
            _healthCheckService = healthCheckService ?? throw new ArgumentNullException(nameof(healthCheckService));
        }

        /// <summary>
        /// Respond to a ping message.
        /// </summary>
        [HttpGet]
        [Route("ping")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RespondPing()
        {
            return await ExecuteAsync(async () =>
            {
                return await Task.FromResult(Ok());
            }, "Ping");
        }

        /// <summary>
        /// Executes a health check.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(HealthCheckValuesDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ExecuteHealthCheck()
        {
            return await ExecuteAsync(async () =>
            {
                var values = _healthCheckService.Execute();
                return await Task.FromResult(Ok(values.ToDto()));
            }, "ExecuteHealthCheck");
        }
    }
}
