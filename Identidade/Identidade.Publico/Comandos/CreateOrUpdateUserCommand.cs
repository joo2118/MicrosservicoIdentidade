using Identidade.Publico.Dtos;

namespace Identidade.Publico.Commands
{
    public class CreateOrUpdateUserCommand
    {
        public string UserId { get; set; }
        public InputUserDto User { get; set; }
        public string RequestUserId { get; set; }
    }
}
