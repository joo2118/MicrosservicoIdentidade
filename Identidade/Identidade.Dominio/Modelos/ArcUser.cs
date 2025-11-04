using System;

namespace Identidade.Dominio.Modelos
{
    public class ArcUser
    {
        public string Login { get; }
        public string Name { get; }
        public string Email { get; }
        public string Password { get; }
        public DateTimeOffset PasswordExpiration { get; }
        public bool PasswordDoesNotExpire { get; }
        public bool Active { get; }
        public string AuthenticationType { get; }
        public string Language { get; }
        public string[] UserGroups { get; }
        public string[] SubstituteUsers { get; }

        public ArcUser(string login, string name, string email, string password, string language, string[] userGroups, string[] substituteUsers, string authenticationType, DateTimeOffset? passwordExpiration, bool passwordDoesNotExpire = false, bool active = true)
        {
            if (string.IsNullOrWhiteSpace(login))
                throw new ArgumentException($"{nameof(login)} cannot be null, empty or white-space.", nameof(login));

            if (string.IsNullOrWhiteSpace(authenticationType))
                throw new ArgumentException($"{nameof(authenticationType)} cannot be null, empty or white-space.", nameof(authenticationType));

            Login = login;
            Name = name;
            Email = email;
            Password = password;
            Language = language;
            UserGroups = userGroups ?? Array.Empty<string>();
            SubstituteUsers = substituteUsers ?? Array.Empty<string>();
            AuthenticationType = authenticationType;
            PasswordExpiration = passwordExpiration ?? DateTimeOffset.MaxValue;
            PasswordDoesNotExpire = passwordDoesNotExpire;
            Active = active;
        }
    }
}