using System;
using System.Collections.Generic;
using System.Text;

namespace Identidade.Publico.Dtos
{
    public class PatchUserDto : UserBaseDto
    {
        public string Password { get; set; }
    }
}
