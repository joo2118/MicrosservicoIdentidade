using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Identidade.Publico.Dtos
{
    [KnownType(typeof(InputPermissionDto))]
    [Serializable]
    public class InputUserGroupDto
    {
        public string Name { get; set; }
        public InputPermissionDto[] Permissions { get; set; }
        public string ArcXml { get; set; }
    }
    
    [Serializable]
    public class OutputUserGroupDto : InputUserGroupDto
    {
        public string Id { get; set; }

        /// <summary>
        /// The Date that the user group has been created, expressed as the Universal Time Coordinated (UTC).
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The Date representing the last time that the user group has been updated, expressed as the Universal Time Coordinated (UTC).
        /// </summary>
        public DateTime LastUpdatedAt { get; set; }
    }
}
