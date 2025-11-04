using Identidade.Publico.Enumerations;
using System;

namespace Identidade.Publico.Dtos
{
    [Serializable]
    public abstract class UserBaseDto
    {
        public string Login { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTimeOffset? PasswordExpiration { get; set; }
        public bool? PasswordDoesNotExpire { get; set; }
        public bool? Active { get; set; }
        public AuthenticationType? AuthenticationType { get; set; }
        public Language? Language { get; set; }

        /// <summary>
        /// The collection of the IDs of the user groups where the user is assigned in.
        /// </summary>
        public string[] UserGroups { get; set; }

        /// <summary>
        /// The collection of the IDs of the substitute users for the user.
        /// </summary>
        public string[] SubstituteUsers { get; set; }
    }
}
