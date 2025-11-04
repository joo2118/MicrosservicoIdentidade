using Identidade.Dominio.Modelos;
using System.Xml.Linq;

namespace Identidade.Dominio.Interfaces
{
    public interface IArcUserXmlWriter
    {
        XElement Write(ArcUser arcUser, string databaseAuthenticationTypeStringValue, string passwordHistory);
    }
}
