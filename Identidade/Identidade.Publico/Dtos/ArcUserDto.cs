using Identidade.Publico.Enumerations;
using Identidade.Publico.Validators;
using System;

namespace Identidade.Publico.Dtos
{
    public class ArcUserDto : UserBaseDto
    {
        public string Codigo { get; set; }

        public string Password { get; set; }

        public ArcUserDto(string email, string password, string passwordExpiration, bool? passwordDoesNotExpire, bool? active, AuthenticationType authenticationType, Language? language, string codigo = null)
        {
            if (authenticationType == Enumerations.AuthenticationType.AzureAD && string.IsNullOrWhiteSpace(email))
                throw new InvalidOperationException($"Email must be provided for authentication type {Enumerations.AuthenticationType.AzureAD}");

            if (authenticationType == Enumerations.AuthenticationType.DatabaseUser && string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException($"Password must be provided for authentication type {authenticationType}");

            if (!string.IsNullOrWhiteSpace(email) && !EmailValidator.Validate(email))
                throw new ArgumentException("Invalid email format", nameof(email));

            if (string.IsNullOrWhiteSpace(passwordExpiration))
                PasswordExpiration = DateTimeOffset.MaxValue;
            else
                PasswordExpiration = DateTimeOffset.Parse(passwordExpiration);

            Email = email;
            Password = password;
            PasswordDoesNotExpire = passwordDoesNotExpire ?? false;
            Active = active ?? true;
            AuthenticationType = authenticationType;
            Language = language ?? Enumerations.Language.Portugues;
            Codigo = codigo;
        }
    }
}