using System.ComponentModel.DataAnnotations.Schema;

namespace Identidade.Dominio.Modelos
{
    public class UserSubstitution
    {
        public User User { get; set; }
        public User SubstituteUser { get; set; }

        public string UserId { get; set; }
        public string SubstituteUserId { get; set; }

        public UserSubstitution() { }

        public UserSubstitution(User user, User substituteUser)
        {
            User = user;
            SubstituteUser = substituteUser;
            UserId = user?.Id;
            SubstituteUserId = substituteUser.Id;
        }
    }
}
