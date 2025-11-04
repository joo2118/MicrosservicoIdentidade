namespace Identidade.Publico.Events
{
    public class UserGroupDeletedEvent
    {
        public string UserGroupId { get; set; }
        public string RequestUserId { get; set; }
    }
}
