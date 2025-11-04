using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Identidade.Infraestrutura.Interfaces;
using Identidade.Publico.Dtos;
using System;
using Identidade.Infraestrutura.Extensoes;

namespace Identidade.RESTAPI.Controllers
{
    /// <summary>
    /// Controller for HealthCheck purpose
    /// </summary>
    [Route("healthcheck")]
    public class HealthCheckController : ControllerBase
    {
        private readonly IHealthCheckService _healthCheckService;

        /// <summary>
        /// Constructor.
        /// </summary>
        public HealthCheckController(IHealthCheckService healthCheckService)
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
        public ActionResult RespondPing()
        {
            return Ok();
        }

        /// <summary>
        /// Executes a health check.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(HealthCheckValuesDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public ActionResult ExecuteHealthCheck()
        {
            var values = _healthCheckService.Execute();
            return Ok(values.ToDto());
        }
    }
}
