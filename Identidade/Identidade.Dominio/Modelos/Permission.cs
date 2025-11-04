using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Identidade.Dominio.Modelos
{
    public class Permission
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public ICollection<UserGroupPermission> UserGroupPermissions { get; set; }
    }
}