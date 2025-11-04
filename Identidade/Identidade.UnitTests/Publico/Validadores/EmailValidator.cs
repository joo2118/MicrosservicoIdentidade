using Identidade.Publico.Validators;
using Xunit;

namespace Identidade.UnitTests.Public.Validators
{
    public class EmailValidatorTests
    {

        [Theory]
        [InlineData("teste@exemplovalido.com", true)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData(" ", false)]
        [InlineData("testeinvalido", false)]
        [InlineData("testeinvalido@home", false)]
        [InlineData("a@b.c", false)]
        [InlineData("teste-invalido[at]home.com", false)]
        [InlineData("teste@invalido.home.place", false)]
        [InlineData("teste.dois@pontos..com", false)]
        [InlineData("teste<>caracter@invalido.com", false)]
        [InlineData("teste@ponto.fim.com.", false)]
        [InlineData("a@10.1.100.1a", false)]
        public void Validate_WhenCalled_ReturnsCorrectResult(string email, bool expectedResult)
        {
            var result = EmailValidator.Validate(email);

            Assert.Equal(expectedResult, result);
        }
    }
}