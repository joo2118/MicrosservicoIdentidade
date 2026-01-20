using Microsoft.AspNetCore.Mvc;
using Identidade.Dominio.Helpers;
using Identidade.Publico.Dtos;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Serilog;
using System.Collections.Generic;
using Identidade.RESTAPI.Controladores;
using Identidade.Infraestrutura.ServicosCliente;

namespace Identidade.RESTAPI.Controllers
{
    public class AuthController : BaseController
    {
        private readonly IAuthClientService _authService;

        public AuthController(IAuthClientService authService, TelemetryClient telemetryClient, ILogger logger)
            : base(telemetryClient, logger)
        {
            _authService = authService;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginInfoDto loginInfoDto)
        {
            return await ExecuteAsync(async () =>
            {
                try
                {
                    if (await _authService.LogIn(loginInfoDto))
                        return Ok();

                    return Unauthorized();
                }
                catch (NotFoundAppException e)
                {
                    return NotFound(e.Errors);
                }
            }, "Login", new Dictionary<string, string> { { "UserName", loginInfoDto?.UserName } });
        }

        [HttpPost]
        public async Task<IActionResult> LogOut()
        {
            return await ExecuteAsync(async () =>
            {
                if (!User.Identity.IsAuthenticated)
                    return NoContent();
                
                await _authService.LogOut();
                return SignOut();
            }, "LogOut");
        }
    }
}
