using Identidade.Dominio.Modelos;
using Identidade.Dominio.Modelos;
using Identidade.Dominio.Writers;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Xunit;

namespace Identidade.UnitTests.Domain.Writers
{
    public class ArcUserXmlWriterTests
    {
        [Fact]
        public void Write_ShouldReturnCorrectXml_WhenArcUserIsValid_And_PasswordHistoryIsNotNullOrEmpty()
        {
            var userGroups = new string[] { "group1", "group2" };
            var substituteUsers = new string[] { "subUser1", "subUser2" };
            var passwordExpiration = DateTimeOffset.Now;
            var arcUser = new ArcUser("login", "name", "email", "password", "language", userGroups, substituteUsers, "authenticationType", passwordExpiration, true, false);
            var databaseAuthenticationTypeStringValue = "TestAuthType";
            var passwordHistory = "password1;password2";
            var arcUserXmlWriter = new ArcUserXmlWriter();

            var result = arcUserXmlWriter.Write(arcUser, databaseAuthenticationTypeStringValue, passwordHistory);

            Assert.NotNull(result);
            Assert.IsType<XElement>(result);
            Assert.Equal(arcUser.Name, result.Attribute(Constants.ArcXml.informalname).Value);
            Assert.Equal(arcUser.Password, result.Attribute(Constants.ArcXml.password).Value);
            Assert.Equal(arcUser.PasswordDoesNotExpire.ToString().ToLower(), result.Attribute(Constants.ArcXml.passwordneverexpires).Value);
            Assert.Equal(arcUser.Active.ToString().ToLower(), result.Attribute(Constants.ArcXml.userlogon).Value);
            Assert.Equal(arcUser.PasswordExpiration.ToString(Constants.cst_yyyy_MM_dd), result.Attribute(Constants.ArcXml.date).Value);
            Assert.Equal(databaseAuthenticationTypeStringValue, result.Attribute(Constants.ArcXml.authentication).Value);
            Assert.Equal(arcUser.Language, result.Attribute(Constants.ArcXml.language).Value);
            Assert.Equal(arcUser.Email, result.Attribute(Constants.ArcXml.email).Value);
        }

        public static IEnumerable<object?[]> GetWriteInvalidPasswordHistoryTestParameters()
        {
            yield return new object?[] { null };
            yield return new object?[] { string.Empty };
            yield return new object?[] { " " };
        }

        [Theory]
        [MemberData(nameof(GetWriteInvalidPasswordHistoryTestParameters))]
        public void Write_InvalidPasswordHistoryTest(string passwordHistory)
        {
            var userGroups = new string[] { "group1", "group2" };
            var substituteUsers = new string[] { "subUser1", "subUser2" };
            var passwordExpiration = DateTimeOffset.Now;
            var arcUser = new ArcUser("login", "name", "email", "password", "language", userGroups, substituteUsers, "authenticationType", passwordExpiration, true, false);
            var databaseAuthenticationTypeStringValue = "TestAuthType";
            var arcUserXmlWriter = new ArcUserXmlWriter();

            var result = arcUserXmlWriter.Write(arcUser, databaseAuthenticationTypeStringValue, passwordHistory);

            Assert.NotNull(result);
            Assert.IsType<XElement>(result);
            Assert.Equal(arcUser.Name, result.Attribute(Constants.ArcXml.informalname).Value);
            Assert.Equal(arcUser.Password, result.Attribute(Constants.ArcXml.password).Value);
            Assert.Equal(arcUser.PasswordDoesNotExpire.ToString().ToLower(), result.Attribute(Constants.ArcXml.passwordneverexpires).Value);
            Assert.Equal(arcUser.Active.ToString().ToLower(), result.Attribute(Constants.ArcXml.userlogon).Value);
            Assert.Equal(arcUser.PasswordExpiration.ToString(Constants.cst_yyyy_MM_dd), result.Attribute(Constants.ArcXml.date).Value);
            Assert.Equal(databaseAuthenticationTypeStringValue, result.Attribute(Constants.ArcXml.authentication).Value);
            Assert.Equal(arcUser.Language, result.Attribute(Constants.ArcXml.language).Value);
            Assert.Equal(arcUser.Email, result.Attribute(Constants.ArcXml.email).Value);
        }
    }
}