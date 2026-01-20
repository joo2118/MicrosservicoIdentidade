using Identidade.Dominio.Servicos;
using Identidade.Infraestrutura.Resilience;
using Polly;
using System;
using System.Threading.Tasks;
using Identidade.Publico.Dtos;

namespace Identidade.Infraestrutura.ServicosCliente
{
    public interface IAuthClientService
    {
        Task<bool> LogIn(LoginInfoDto loginInfoDto);
        Task LogOut();
    }

    public class AuthClientService : IAuthClientService
    {
        private readonly ISignInManager _signInManager;
        private readonly ResiliencePipeline _pipeline;

        public AuthClientService(ISignInManager signInManager)
        {
            _signInManager = signInManager;
            _pipeline = FabricaPipelineResiliencia.Create();
        }

        public Task<bool> LogIn(LoginInfoDto loginInfoDto) =>
            ExecuteResilientAsync(() => _signInManager.LogIn(loginInfoDto.UserName, loginInfoDto.Password));

        public Task LogOut() =>
            ExecuteResilientAsync(() => _signInManager.LogOut());

        private Task<T> ExecuteResilientAsync<T>(Func<Task<T>> action) =>
            _pipeline.ExecuteAsync(async _ => await action().ConfigureAwait(false)).AsTask();

        private Task ExecuteResilientAsync(Func<Task> action) =>
            _pipeline.ExecuteAsync(async _ => await action().ConfigureAwait(false)).AsTask();
    }
}
