using Identidade.Infraestrutura.Interfaces;
using System;

namespace Identidade.Infraestrutura.Adaptadores
{
    public class EnvironmentAdapter : IEnvironmentAdapter
    {
        public int ProcessId => Environment.ProcessId;
        public string CommandLine => Environment.CommandLine;
    }
}
