using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Repositorios;
using System;
using System.Threading.Tasks;

namespace Identidade.Dominio.Servicos
{
    public interface ISignInManager
    {
        Task<bool> LogIn(string userName, string password);
        Task LogOut();
    }

    public class SignInManagerService : ISignInManager
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogInService _logInService;

        public SignInManagerService(IUserRepository userRepository, ILogInService logInService)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logInService = logInService ?? throw new ArgumentNullException(nameof(logInService));
        }

        public async Task<bool> LogIn(string userName, string password)
        {
            var user = await _userRepository.GetByName(userName);
            if (user != null)
            {
                await _logInService.LogIn(user);
                return true;
            }
            return false;
        }

        public Task LogOut() =>
            _logInService.LogOut();
    }
}
