using System;
using Identidade.Dominio.Extensoes;
using Xunit;

namespace Identidade.UnitTests.Domain.Extensions
{
    public class ExceptionExtensionsTests
    {
        [Fact]
        public void GetAllMessages_WithInnerException_ReturnsAllMessages()
        {
            var innerExceptionMessage = "This is the inner exception message.";
            var outerExceptionMessage = "This is the outer exception message.";
            var innerException = new Exception(innerExceptionMessage);
            var outerException = new Exception(outerExceptionMessage, innerException);

            var messages = outerException.GetAllMessages();

            Assert.Contains(outerExceptionMessage, messages);
            Assert.Contains(innerExceptionMessage, messages);
        }

        [Fact]
        public void NullException_Test()
        {
            var messages = ((Exception)null).GetAllMessages();

            Assert.Empty(messages);
        }
    }
}