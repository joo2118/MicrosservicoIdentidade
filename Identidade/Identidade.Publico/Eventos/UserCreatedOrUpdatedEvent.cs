using Identidade.Publico.Dtos;
using Identidade.Publico.Enumerations;

namespace Identidade.Publico.Events
{
    public class UserCreatedOrUpdatedEvent
    {
        public OutputUserDto User { get; set; }
        public string PasswordHash { get; set; }
        public AuthenticationType AuthenticationType { get; set; }
        public string HashArc {get; set;}
        public string RequestUserId { get; set; }
    }
}
