using Identidade.Dominio.Modelos;

namespace Identidade.Dominio.Interfaces
{
    public interface ICredentialsFactory
    {
        Credentials Create(string authorizationToken, string requestUser = null);
    }
}
