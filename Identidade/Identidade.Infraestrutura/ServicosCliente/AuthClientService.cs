using Identidade.Dominio.Servicos;
using Identidade.Publico.Dtos;
using System.Threading.Tasks;

namespace Identidade.Infraestrutura.ClientServices
{
    public interface IAuthClientService
    {
        Task<bool> LogIn(LoginInfoDto loginInfoDto);
        Task LogOut();
    }

    public class AuthClientService : IAuthClientService
    {
        private readonly ISignInManager _signInManager;

        public AuthClientService(ISignInManager signInManager)
        {
            _signInManager = signInManager;
        }

        public async Task<bool> LogIn(LoginInfoDto loginInfoDto) =>
            await _signInManager.LogIn(loginInfoDto.UserName, loginInfoDto.Password);
        
        public async Task LogOut() =>
            await _signInManager.LogOut();
    }
}
