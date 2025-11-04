using System.Xml.Linq;
using Identidade.Dominio.Extensions;
using Xunit;

namespace Identidade.UnitTests.Domain.Extensions
{
    public class ExtensionXmlTests
    {
        [Fact]
        public void SetAttribute_ShouldSetBooleanAttributeCorrectly()
        {
            var element = new XElement("Test");
            element.SetAttribute("Attribute", true);

            Assert.Equal("true", element.Attribute("Attribute").Value);
        }

        [Fact]
        public void SetAttribute_ShouldSetStringAttributeCorrectly()
        {
            var element = new XElement("Test");
            element.SetAttribute("Attribute", "value");

            Assert.Equal("value", element.Attribute("Attribute").Value);
        }

        [Fact]
        public void SetAttribute_ShouldNotSetAttribute_WhenValueIsNull()
        {
            var element = new XElement("Test");
            element.SetAttribute("Attribute", (string)null);

            Assert.Null(element.Attribute("Attribute"));
        }

        [Fact]
        public void SetAttribute_ShouldUpdateExistingAttribute()
        {
            var element = new XElement("Test", new XAttribute("Attribute", "oldValue"));
            element.SetAttribute("Attribute", "newValue");

            Assert.Equal("newValue", element.Attribute("Attribute").Value);
        }
    }
}