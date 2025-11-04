using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Identidade.Dominio.Modelos
{
    public class User : IdentityUser
    {
        public string Name { get; set; }
        public DateTimeOffset? PasswordExpiration { get; set; }
        public bool? PasswordDoesNotExpire { get; set; }
        public string AuthenticationType { get; set; }
        public string Language { get; set; }

        /// <summary>
        /// The Date that the user has been created, expressed as the Universal Time Coordinated (UTC).
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The Date representing the last time that the user has been updated, expressed as the Universal Time Coordinated (UTC).
        /// </summary>
        public DateTime LastUpdatedAt { get; set; }

        public ICollection<UserGroupUser> UserGroupUsers { get; set; } = new List<UserGroupUser>();
        public ICollection<UserSubstitution> UserSubstitutions { get; set; } = new List<UserSubstitution>();

        /// <summary>
        /// A string containing the passwordHash history separated by ";", including the current passwordHash.
        /// </summary>
        public string PasswordHistory { get; set; } = string.Empty;
    }
}
