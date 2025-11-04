using Identidade.Dominio.Modelos;
using System.Threading.Tasks;

namespace Identidade.Dominio.Interfaces
{
    public interface IUserGroupRepository
    {
        Task<UserGroup[]> GetUserGroups(string[] userGroupIds);
        Task<User[]> GetUsers(string[] userIds);
    }
}
