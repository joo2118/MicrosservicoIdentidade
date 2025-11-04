using System;

namespace Identidade.Publico.Dtos
{
    [Serializable]
    public class InputUserDto : UserBaseDto
    {
        public string Password { get; set; }
        public string ArcXml { get; set; }
    }
}
