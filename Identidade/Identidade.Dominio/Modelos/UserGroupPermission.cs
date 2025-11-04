namespace Identidade.Dominio.Modelos
{
    public class UserGroupPermission
    {
        public UserGroup UserGroup { get; set; }
        public Permission Permission { get; set; }

        /// <summary>
        /// The sum of the int values of the permission operations.
        /// </summary>
        public int PermissionOperations { get; set; }

        public string UserGroupId { get; set; }
        public string PermissionId { get; set; }

        public UserGroupPermission() { }

        public UserGroupPermission(UserGroup userGroup, Permission permission, int permissionOperations)
        {
            UserGroup = userGroup;
            Permission = permission;
            UserGroupId = userGroup?.Id;
            PermissionId = permission.Id;
            PermissionOperations = permissionOperations;
        }
    }
}
