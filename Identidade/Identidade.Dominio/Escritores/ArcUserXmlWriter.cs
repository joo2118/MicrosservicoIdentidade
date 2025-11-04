using Identidade.Dominio.Extensions;
using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Modelos;
using Identidade.Dominio.Modelos;
using System.Xml.Linq;

namespace Identidade.Dominio.Writers
{
    public class ArcUserXmlWriter : IArcUserXmlWriter
    {
        public XElement Write(ArcUser arcUser, string databaseAuthenticationTypeStringValue, string passwordHistory)
        {
            XElement xmlUsuario = new XElement(Constants.ArcXml.user);

            xmlUsuario.SetAttribute(Constants.ArcXml.informalname, arcUser.Name);
            xmlUsuario.SetAttribute(Constants.ArcXml.password, arcUser.Password);
            xmlUsuario.SetAttribute(Constants.ArcXml.passwordneverexpires, arcUser.PasswordDoesNotExpire);
            xmlUsuario.SetAttribute(Constants.ArcXml.userlogon, arcUser.Active);
            xmlUsuario.SetAttribute(Constants.ArcXml.date, arcUser.PasswordExpiration.ToString(Constants.cst_yyyy_MM_dd));
            xmlUsuario.SetAttribute(Constants.ArcXml.authentication, databaseAuthenticationTypeStringValue);
            xmlUsuario.SetAttribute(Constants.ArcXml.language, arcUser.Language);

            if (!string.IsNullOrWhiteSpace(arcUser.Email))
                xmlUsuario.SetAttribute(Constants.ArcXml.email, arcUser.Email);

            xmlUsuario.Add(CriaElementoGrupos(arcUser));
            xmlUsuario.Add(CriaElementoSenhas(passwordHistory));

            return xmlUsuario;
        }

        private XElement CriaElementoGrupos(ArcUser arcUser)
        {
            XElement newGroups = new XElement(Constants.ArcXml.usergroups);

            foreach (var codigoGrupo in arcUser.UserGroups)
            {
                XElement group = new XElement(Constants.ArcXml.usergroup);
                group.SetAttribute(Constants.ArcXml.code, codigoGrupo);

                newGroups.Add(group);
            }

            return newGroups;
        }

        private XElement CriaElementoSenhas(string passwordHistory)
        {
            XElement passwordHistoryElement = new XElement(Constants.ArcXml.passwordshistory);

            if (!string.IsNullOrWhiteSpace(passwordHistory))
                foreach (string s in passwordHistory.Split(';'))
                {
                    XElement tagPassword = new XElement(Constants.ArcXml.password);
                    tagPassword.SetAttribute(Constants.ArcXml.code, s);
                    passwordHistoryElement.Add(tagPassword);
                }

            return passwordHistoryElement;
        }
    }
}