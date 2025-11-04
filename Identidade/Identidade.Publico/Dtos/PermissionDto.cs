using Identidade.Publico.Enumerations;
using System;

namespace Identidade.Publico.Dtos
{
    [Serializable]
    public class InputPermissionDto
    {
        public string Id { get; set; }
        public string[] Operations { get; set; } = new[] { PermissionOperation.All.ToString() };
    }

    [Serializable]
    public class OutputPermissionDto : InputPermissionDto
    {
        public string Name { get; set; }
    }
}
