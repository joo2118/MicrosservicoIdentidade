using System;

namespace Identidade.Publico.Dtos
{
    [Serializable]
    public class LoginInfoDto
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
