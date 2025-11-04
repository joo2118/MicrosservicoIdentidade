using System;
using System.ComponentModel.DataAnnotations;

namespace Identidade.Dominio.Modelos
{
    [Serializable]
    public class MessageInformation
    {
        [Key]
        public string MessageId { get; set; }
    }
}
