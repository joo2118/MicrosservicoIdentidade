using Xunit;
using Identidade.Publico.Dtos;
using Identidade.Publico.Enumerations;
using System;
using System.Collections.Generic;

namespace Identidade.UnitTests.Publico.Dtos
{
    public class ArcUserDtoTests
    {
        [Fact]
        public void Constructor_ShouldThrowInvalidOperationException_WhenAuthenticationTypeNeedPasswordAndPasswordEmpty()
        {
            Assert.Throws<InvalidOperationException>(() => new ArcUserDto("email@test.com", null, null, null, null, AuthenticationType.DatabaseUser, null));
            Assert.Throws<InvalidOperationException>(() => new ArcUserDto("email@test.com", string.Empty, null, null, null, AuthenticationType.DatabaseUser, null));
            Assert.Throws<InvalidOperationException>(() => new ArcUserDto("email@test.com", " ", null, null, null, AuthenticationType.DatabaseUser, null));
        }

        [Fact]
        public void Constructor_ShouldThrowInvalidOperationException_WhenAuthenticationTypeIsAzureADAndEmailIsEmpty()
        {
            Assert.Throws<InvalidOperationException>(() => new ArcUserDto(string.Empty, "password", null, null, null, AuthenticationType.AzureAD, null));
            Assert.Throws<InvalidOperationException>(() => new ArcUserDto(" ", "password", null, null, null, AuthenticationType.AzureAD, null));
            Assert.Throws<InvalidOperationException>(() => new ArcUserDto(null, "password", null, null, null, AuthenticationType.AzureAD, null));
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentException_WhenPasswordExpirationIsInvalid()
        {
            Assert.Throws<FormatException>(() => new ArcUserDto("email@test.com", "password", "InvalidPasswordExpiration", null, null, AuthenticationType.DatabaseUser, null));
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentException_WhenEmailIsInvalid()
        {
            Assert.Throws<ArgumentException>(() => new ArcUserDto("testeinvalido", "password", null, null, null, AuthenticationType.DatabaseUser, null));
            Assert.Throws<ArgumentException>(() => new ArcUserDto("testeinvalido@home", "password", null, null, null, AuthenticationType.DatabaseUser, null));
            Assert.Throws<ArgumentException>(() => new ArcUserDto("a@b.c", "password", null, null, null, AuthenticationType.DatabaseUser, null));
            Assert.Throws<ArgumentException>(() => new ArcUserDto("teste-invalido[at]home.com", "password", null, null, null, AuthenticationType.DatabaseUser, null));
            Assert.Throws<ArgumentException>(() => new ArcUserDto("teste@invalido.home.place", "password", null, null, null, AuthenticationType.DatabaseUser, null));
            Assert.Throws<ArgumentException>(() => new ArcUserDto("teste.dois@pontos..com", "password", null, null, null, AuthenticationType.DatabaseUser, null));
            Assert.Throws<ArgumentException>(() => new ArcUserDto("teste<>caracter@invalido.com", "password", null, null, null, AuthenticationType.DatabaseUser, null));
            Assert.Throws<ArgumentException>(() => new ArcUserDto("teste@ponto.fim.com.", "password", null, null, null, AuthenticationType.DatabaseUser, null));
            Assert.Throws<ArgumentException>(() => new ArcUserDto("a@10.1.100.1a", "password", null, null, null, AuthenticationType.DatabaseUser, null));
        }

        public static IEnumerable<object[]> ConstructorShouldSetPropertiesCorrectlyTestParameters()
        {

            yield return new object[] { string.Empty, DateTimeOffset.MaxValue, "TesteCodigo" };
            yield return new object[] { " ", DateTimeOffset.MaxValue, null};
            yield return new object[] { null, DateTimeOffset.MaxValue, null };
            yield return new object[] { new DateTimeOffset(2023, 9, 8, 1, 2, 3, TimeSpan.FromHours(-3)).ToString(), new DateTimeOffset(2023, 9, 8, 1, 2, 3, TimeSpan.FromHours(-3)), null };
        }

        [Theory]
        [MemberData(nameof(ConstructorShouldSetPropertiesCorrectlyTestParameters))]
        public void Constructor_ShouldSetPropertiesCorrectly(string passwordExpiration, DateTimeOffset expectedDateTimeOffset, string codigo)
        {
            var arcUserDto = new ArcUserDto("email@test.com", "password", passwordExpiration, false, true, AuthenticationType.DatabaseUser, Language.Ingles, codigo);

            Assert.Equal("email@test.com", arcUserDto.Email);
            Assert.Equal("password", arcUserDto.Password);
            Assert.Equal(expectedDateTimeOffset, arcUserDto.PasswordExpiration);
            Assert.Equal(false, arcUserDto.PasswordDoesNotExpire);
            Assert.Equal(true, arcUserDto.Active);
            Assert.Equal(AuthenticationType.DatabaseUser, arcUserDto.AuthenticationType);
            Assert.Equal(Language.Ingles, arcUserDto.Language);
        }

        [Fact]
        public void Constructor_ShouldSetDefaultValues_WhenNullValuesArePassed()
        {
            var arcUserDto = new ArcUserDto(null, null, null, null, null, AuthenticationType.ActiveDirectory, null);

            Assert.Equal(DateTimeOffset.MaxValue, arcUserDto.PasswordExpiration);
            Assert.Equal(false, arcUserDto.PasswordDoesNotExpire);
            Assert.Equal(true, arcUserDto.Active);
            Assert.Equal(Language.Portugues, arcUserDto.Language);
        }
    }
}