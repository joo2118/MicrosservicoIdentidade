using System.Xml;
using System.Xml.Linq;

namespace Identidade.Dominio.Extensions
{
    public static class ExtensionXml
    {
        public static XElement SetAttribute(this XElement elemento, string nomeAtributo, bool valor)
        {
            return SetAttribute(elemento, nomeAtributo, XmlConvert.ToString(valor));
        }

        /// <summary>
        /// Altera um valor de texto de um atributo xml
        /// </summary>
        public static XElement SetAttribute(this XElement elemento, string nomeAtributo, string valor)
        {
            if (valor == null)
                return elemento;

            XAttribute atributo = elemento.Attribute(nomeAtributo);

            if (atributo != null)
                atributo.Value = valor;
            else
                elemento.Add(new XAttribute(nomeAtributo, valor));

            return elemento;
        }
    }
}
