namespace Identidade.Publico.Events
{
    public class UserDeletedEvent
    {
        public string UserId { get; set; }
        public string RequestUserId { get; set; }
    }
}
