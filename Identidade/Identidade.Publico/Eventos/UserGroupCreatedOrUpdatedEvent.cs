using Identidade.Publico.Dtos;

namespace Identidade.Publico.Events
{
    public class UserGroupCreatedOrUpdatedEvent
    {
        public OutputUserGroupDto UserGroup { get; set; }
        public string RequestUserId { get; set; }
    }
}
