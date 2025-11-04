using Identidade.Infraestrutura.Helpers;
using Identidade.Publico.Dtos;
using Identidade.Publico.Enumerations;
using System;
using Xunit;

namespace Identidade.UnitTests.Infraestrutura.Helpers
{
    public class PatchUserMergerTests
    {
        [Fact]
        public void Merge_Test()
        {
            var currentUser = CreateCurrentUser(out var currentPassword);

            var patchUser = new PatchUserDto()
            {
                Login = "NewLogin",
                Name = "NewName",
                Email = "New@Email.com",
                Password = "NewPassword",
                PasswordExpiration = new DateTime(2023, 4, 5),
                PasswordDoesNotExpire = true,
                Active = false,
                AuthenticationType = AuthenticationType.DatabaseUser,
                Language = Language.Portugues,
                UserGroups = new string[] { "NewGroup1", "NewGroup2" },
                SubstituteUsers = new string[] { "NewSubstitute1", "NewSubstitute2" }
            };

            var merger = new PatchUserMerger();
            var actual = merger.Merge(currentUser, currentPassword, patchUser);

            Assert.Equal(patchUser.Login, actual.Login);
            Assert.Equal(patchUser.Name, actual.Name);
            Assert.Equal(patchUser.Email, actual.Email);
            Assert.Equal(patchUser.Password, actual.Password);
            Assert.Equal(patchUser.PasswordExpiration, actual.PasswordExpiration);
            Assert.Equal(patchUser.PasswordDoesNotExpire, actual.PasswordDoesNotExpire);
            Assert.Equal(patchUser.Active, actual.Active);
            Assert.Equal(patchUser.AuthenticationType, actual.AuthenticationType);
            Assert.Equal(patchUser.Language, actual.Language);
            Assert.Equal(patchUser.UserGroups, actual.UserGroups);
            Assert.Equal(patchUser.SubstituteUsers, actual.SubstituteUsers);
        }

        [Fact]
        public void Merge_NoUpdates_Test()
        {
            var currentUser = CreateCurrentUser(out var currentPassword);

            var patchUser = new PatchUserDto();

            var merger = new PatchUserMerger();
            var actual = merger.Merge(currentUser, currentPassword, patchUser);

            Assert.Equal(currentUser.Login, actual.Login);
            Assert.Equal(currentUser.Name, actual.Name);
            Assert.Equal(currentUser.Email, actual.Email);
            Assert.Equal(currentPassword, actual.Password);
            Assert.Equal(currentUser.PasswordExpiration, actual.PasswordExpiration);
            Assert.Equal(currentUser.PasswordDoesNotExpire, actual.PasswordDoesNotExpire);
            Assert.True(actual.Active);
            Assert.Equal(currentUser.AuthenticationType, actual.AuthenticationType);
            Assert.Equal(currentUser.Language, actual.Language);
            Assert.Equal(currentUser.UserGroups, actual.UserGroups);
            Assert.Equal(currentUser.SubstituteUsers, actual.SubstituteUsers);
        }

        [Fact]
        public void Merge_RemoveOptionalFields_Test()
        {
            var currentUser = CreateCurrentUser(out var currentPassword);
            currentUser.AuthenticationType = AuthenticationType.ActiveDirectory;

            var patchUser = new PatchUserDto()
            {
                Name = string.Empty,
                Email = string.Empty,
                Password = string.Empty,
                UserGroups = new string[] { },
                SubstituteUsers = new string[] { }
            };

            var merger = new PatchUserMerger();
            var actual = merger.Merge(currentUser, currentPassword, patchUser);

            Assert.Equal(currentUser.Login, actual.Login);
            Assert.Equal(patchUser.Name, actual.Name);
            Assert.Equal(patchUser.Email, actual.Email);
            Assert.Equal(patchUser.Password, actual.Password);
            Assert.Equal(currentUser.PasswordExpiration, actual.PasswordExpiration);
            Assert.Equal(currentUser.PasswordDoesNotExpire, actual.PasswordDoesNotExpire);
            Assert.Equal(currentUser.Active, actual.Active);
            Assert.Equal(currentUser.AuthenticationType, actual.AuthenticationType);
            Assert.Equal(currentUser.Language, actual.Language);
            Assert.Equal(patchUser.UserGroups, actual.UserGroups);
            Assert.Equal(patchUser.SubstituteUsers, actual.SubstituteUsers);
        }

        private OutputUserDto CreateCurrentUser(out string currentPassword)
        {
            currentPassword = "ExistingPassword";

            return new OutputUserDto()
            {
                Login = "ExistingLogin",
                Name = "ExistingName",
                Email = "Existing@Email.com",
                PasswordExpiration = new DateTime(2021, 2, 3),
                PasswordDoesNotExpire = false,
                Active = true,
                AuthenticationType = AuthenticationType.DatabaseUser,
                Language = Language.Ingles,
                UserGroups = new string[] { "ExistingGroup1", "ExistingGroup2" },
                SubstituteUsers = new string[] { "ExistingSubstitute1", "ExistingSubstitute2" }
            };
        }
    }
}
