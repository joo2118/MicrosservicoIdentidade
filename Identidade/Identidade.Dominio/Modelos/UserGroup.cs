using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Identidade.Dominio.Modelos
{
    public class UserGroup
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public ICollection<UserGroupUser> UserGroupUsers { get; set; }
        public ICollection<UserGroupPermission> UserGroupPermissions { get; set; }

        /// <summary>
        /// The Date that the user group has been created, expressed as the Universal Time Coordinated (UTC).
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The Date representing the last time that the user group has been updated, expressed as the Universal Time Coordinated (UTC).
        /// </summary>
        public DateTime LastUpdatedAt { get; set; }
    }
}
