using Identidade.Dominio.Modelos;
using System;
using Xunit;

namespace Identidade.UnitTests.Domain.Models
{
    public class ArcUserTests
    {
        [Fact]
        public void Constructor_ShouldThrowArgumentException_WhenLoginIsNullOrWhiteSpace()
        {
            Assert.Throws<ArgumentException>(() => new ArcUser(null, "name", "email", "password", "language", null, null, "authenticationType", null));
            Assert.Throws<ArgumentException>(() => new ArcUser(string.Empty, "name", "email", "password", "language", null, null, "authenticationType", null));
            Assert.Throws<ArgumentException>(() => new ArcUser(" ", "name", "email", "password", "language", null, null, "authenticationType", null));
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentException_WhenAuthenticationTypeIsNullOrWhiteSpace()
        {
            Assert.Throws<ArgumentException>(() => new ArcUser("login", "name", "email", "password", "language", null, null, null, null));
            Assert.Throws<ArgumentException>(() => new ArcUser("login", "name", "email", "password", "language", null, null, string.Empty, null));
            Assert.Throws<ArgumentException>(() => new ArcUser("login", "name", "email", "password", "language", null, null, " ", null));
        }

        [Fact]
        public void Constructor_ShouldSetPropertiesCorrectly()
        {
            var userGroups = new string[] { "group1", "group2" };
            var substituteUsers = new string[] { "subUser1", "subUser2" };
            var passwordExpiration = DateTimeOffset.Now;

            var arcUser = new ArcUser("login", "name", "email", "password", "language", userGroups, substituteUsers, "authenticationType", passwordExpiration, true, false);

            Assert.NotNull(arcUser);
            Assert.Equal("login", arcUser.Login);
            Assert.Equal("name", arcUser.Name);
            Assert.Equal("email", arcUser.Email);
            Assert.Equal("password", arcUser.Password);
            Assert.Equal("language", arcUser.Language);
            Assert.Equal(userGroups, arcUser.UserGroups);
            Assert.Equal(substituteUsers, arcUser.SubstituteUsers);
            Assert.Equal("authenticationType", arcUser.AuthenticationType);
            Assert.Equal(passwordExpiration, arcUser.PasswordExpiration);
            Assert.True(arcUser.PasswordDoesNotExpire);
            Assert.False(arcUser.Active);
        }

        [Fact]
        public void Constructor_ShouldSetDefaultValues_WhenUserGroupsIsNull()
        {
            var arcUser = new ArcUser("login", "name", "email", "password", "language", null, new string[] { "subUser1", "subUser2" }, "authenticationType", DateTimeOffset.Now);
            Assert.NotNull(arcUser.UserGroups);
            Assert.Empty(arcUser.UserGroups);
        }

        [Fact]
        public void Constructor_ShouldSetDefaultValues_WhenSubstituteUsersIsNull()
        {
            var arcUser = new ArcUser("login", "name", "email", "password", "language", new string[] { "group1", "group2" }, null, "authenticationType", DateTimeOffset.Now);
            Assert.NotNull(arcUser.SubstituteUsers);
            Assert.Empty(arcUser.SubstituteUsers);
        }

        [Fact]
        public void Constructor_ShouldSetDefaultValues_WhenPasswordExpirationIsNull()
        {
            var arcUser = new ArcUser("login", "name", "email", "password", "language", new string[] { "group1", "group2" }, new string[] { "subUser1", "subUser2" }, "authenticationType", null);
            Assert.Equal(DateTimeOffset.MaxValue, arcUser.PasswordExpiration);
        }
    }
}
