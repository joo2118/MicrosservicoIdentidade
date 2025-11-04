using Microsoft.AspNetCore.Mvc;
using Identidade.Dominio.Helpers;
using Identidade.Infraestrutura.ClientServices;
using Identidade.Publico.Dtos;
using System.Threading.Tasks;

namespace Identidade.RESTAPI.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly IAuthClientService _authService;

        public AuthController(IAuthClientService authService)
        {
            _authService = authService;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginInfoDto loginInfoDto)
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
        }

        [HttpPost]
        public async Task<IActionResult> LogOut()
        {
            if (!User.Identity.IsAuthenticated)
                return NoContent();
            
            await _authService.LogOut();
            return SignOut();
        }
    }
}
