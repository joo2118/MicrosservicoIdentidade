using Identidade.Dominio.Helpers;
using System.Linq;
using Xunit;

namespace Identidade.UnitTests
{
    public class PasswordValidatorTests
    {
        private readonly PasswordValidator _passwordValidator = new PasswordValidator();
        
        [Fact]
        public void Validade_NullPassword_ThrowsAppException()
        {
            string password = null;

            var exception = Assert.Throws<AppException>(() => _passwordValidator.Validate(password));
            
            Assert.Single(exception.Errors);
            Assert.Equal("The password can not be null.", exception.Errors.Single());
        }

        [Theory]
        [InlineData("")]
        [InlineData("123")]
        public void Validade_EmptyOrShortPasswordName_ThrowsAppException(string password)
        {
            var exception = Assert.Throws<AppException>(() => _passwordValidator.Validate(password));
            
            Assert.Single(exception.Errors);
            Assert.Equal("The password should contain at least 6 characters.", exception.Errors.Single());
        }
        
        [Fact]
        public void Validade_ValidPassword_ValidatesSuccessfully()
        {
            string password = "teste1234";

            var validationResult = _passwordValidator.Validate(password);

            Assert.True(validationResult.IsValid);
        }
    }
}
