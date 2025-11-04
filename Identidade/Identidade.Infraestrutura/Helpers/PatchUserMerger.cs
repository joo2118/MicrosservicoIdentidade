using Identidade.Publico.Dtos;
using Identidade.Publico.Enumerations;

namespace Identidade.Infraestrutura.Helpers
{
    public interface IPatchUserMerger
    {
        ArcUserDto Merge(OutputUserDto currentUser, string password, PatchUserDto patchUser);
    }

    public class PatchUserMerger : IPatchUserMerger
    {
        public ArcUserDto Merge(OutputUserDto currentUser, string password, PatchUserDto patchUser)
        {
            return new ArcUserDto(
                email: patchUser.Email ?? currentUser.Email,
                password: patchUser.Password ?? password,
                passwordExpiration: (patchUser.PasswordExpiration ?? currentUser.PasswordExpiration)?.ToString(),
                passwordDoesNotExpire: patchUser.PasswordDoesNotExpire ?? currentUser.PasswordDoesNotExpire,
                active: patchUser.Active ?? currentUser.Active,
                authenticationType: patchUser.AuthenticationType ?? currentUser.AuthenticationType ?? AuthenticationType.ActiveDirectory,
                language: patchUser.Language ?? currentUser.Language)
            {
                Login = !string.IsNullOrWhiteSpace(patchUser.Login) ? patchUser.Login : currentUser.Login,
                Name = patchUser.Name ?? currentUser.Name,
                UserGroups = patchUser.UserGroups ?? currentUser.UserGroups,
                SubstituteUsers = patchUser.SubstituteUsers ?? currentUser.SubstituteUsers
            };
        }
    }
}
