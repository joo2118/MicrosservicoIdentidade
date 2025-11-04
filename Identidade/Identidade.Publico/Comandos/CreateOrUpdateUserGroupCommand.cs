using Identidade.Publico.Dtos;

namespace Identidade.Publico.Commands
{
    public class CreateOrUpdateUserGroupCommand
    {
        public string UserGroupId { get; set; }
        public InputUserGroupDto UserGroup { get; set; }
        public string RequestUserId { get; set; }
    }
}
