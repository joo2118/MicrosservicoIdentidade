using Microsoft.Extensions.Configuration;
using Identidade.Infraestrutura.Entidades;
using Identidade.Infraestrutura.Interfaces;
using System;
using System.IO;
using System.Collections.Generic;

namespace Identidade.Infraestrutura.Servicos
{
    public class HealthCheckService : IHealthCheckService
    {
        private readonly IEnvironmentAdapter _environment;
        private readonly IConfiguration _configuration;

        public HealthCheckService(IEnvironmentAdapter environment, IConfiguration configuration)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public HealthCheckValues Execute()
        {
            return new HealthCheckValues(
                processId: _environment.ProcessId.ToString(),
                processName: GetExecutableProcessName(),
                configItems: new Dictionary<string, string>());
        }

        private string GetExecutableProcessName()
        {
            string commandLine = _environment.CommandLine;
            string executablePath = commandLine.Split(' ')[0];
            var name = Path.GetFileNameWithoutExtension(executablePath);
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("Could not determine the executable process name from the command line.");

            return name;
        }
    }
}
