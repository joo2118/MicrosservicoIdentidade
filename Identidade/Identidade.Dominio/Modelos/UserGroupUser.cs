namespace Identidade.Dominio.Modelos
{
    public class UserGroupUser
    {
        public UserGroup UserGroup { get; set; }
        public User User { get; set; }

        public string UserGroupId { get; set; }
        public string UserId { get; set; }

        public UserGroupUser() { }

        public UserGroupUser(UserGroup userGroup, User user)
        {
            UserGroup = userGroup;
            User = user;
            UserGroupId = userGroup?.Id;
            UserId = user?.Id;
        }
    }
}
