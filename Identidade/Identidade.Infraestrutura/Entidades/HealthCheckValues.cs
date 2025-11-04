using System;
using System.Collections.Generic;

namespace Identidade.Infraestrutura.Entidades
{
    public class HealthCheckValues
    {
        public string ProcessId { get; }
        public string ProcessName { get; }
        public IReadOnlyDictionary<string, string> ConfigItems { get; }

        public HealthCheckValues(string processId, string processName, IReadOnlyDictionary<string, string> configItems)
        {
            ProcessId = !string.IsNullOrWhiteSpace(processId) ? processId : throw new ArgumentException($"{nameof(processId)} cannot be null, empty or white-space.", paramName: nameof(processId));
            ProcessName = !string.IsNullOrWhiteSpace(processName) ? processName : throw new ArgumentException($"{nameof(processName)} cannot be null, empty or white-space.", paramName: nameof(processName));
            ConfigItems = configItems ?? new Dictionary<string, string>();
        }
    }
}
