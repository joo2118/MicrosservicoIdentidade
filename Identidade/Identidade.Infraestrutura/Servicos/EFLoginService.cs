using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Modelos;

namespace Identidade.Infraestrutura.Services
{
    internal class EFLoginService : ILogInService
    {
        private readonly SignInManager<User> _signInManager;

        public EFLoginService(SignInManager<User> signInManager)
        {
            _signInManager = signInManager;
        }
        
        public async Task LogIn(User user) =>
            await _signInManager.SignInAsync(user, true);

        public async Task LogOut() =>
            await _signInManager.SignOutAsync();
    }
}
