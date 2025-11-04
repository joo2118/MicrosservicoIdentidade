using System;

namespace Identidade.Publico.Dtos
{
    [Serializable]
    public class OutputUserDto : UserBaseDto
    {
        public string Id { get; set; }

        /// <summary>
        /// The Date that the user has been created, expressed as the Universal Time Coordinated (UTC).
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The Date representing the last time that the user has been updated, expressed as the Universal Time Coordinated (UTC).
        /// </summary>
        public DateTime LastUpdatedAt { get; set; }
    }
}