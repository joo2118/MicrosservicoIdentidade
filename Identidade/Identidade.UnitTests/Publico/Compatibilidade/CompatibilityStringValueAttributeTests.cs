using Identidade.Publico.Compatibility;
using System;
using Xunit;

namespace Identidade.UnitTests.Public.Compatibility
{
    public class CompatibilityStringValueAttributeTests
    {
        [Fact]
        public void Constructor_ShouldThrowArgumentException_WhenStringValueIsNullOrWhiteSpace()
        {
            Assert.Throws<ArgumentException>(() => new CompatibilityStringValueAttribute(null));
            Assert.Throws<ArgumentException>(() => new CompatibilityStringValueAttribute(""));
            Assert.Throws<ArgumentException>(() => new CompatibilityStringValueAttribute(" "));
        }

        [Fact]
        public void Constructor_ShouldSetStringValuePropertyCorrectly()
        {
            var attribute = new CompatibilityStringValueAttribute("test");

            Assert.Equal("test", attribute.StringValue);
        }
    }
}