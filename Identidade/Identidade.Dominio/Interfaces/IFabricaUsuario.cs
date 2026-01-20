using System.Threading;
using System.Threading.Tasks;
using Identidade.Dominio.Modelos;
using Identidade.Publico.Dtos;

namespace Identidade.Dominio.Interfaces
{
    public interface IFabricaUsuario
    {
        Task<User> MapearParaUsuarioAsync(InputUserDto dto, string originalUserLogin = null, CancellationToken cancellationToken = default);

        Task<User> MapearParaUsuarioAsync(ArcUserDto dto, string originalUserLogin = null, CancellationToken cancellationToken = default);

        OutputUserDto MapearParaDtoSaidaUsuario(User user);

        ArcUser MapearParaArcUser(ArcUserDto dto);

        void ValidarArcUserDto(ArcUserDto dto);
    }
}
