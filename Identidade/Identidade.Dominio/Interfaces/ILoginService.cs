using Identidade.Dominio.Modelos;
using System.Threading.Tasks;

namespace Identidade.Dominio.Interfaces
{
    public interface ILogInService
    {
        Task LogIn(User user);
        Task LogOut();
    }
}
