using System;

namespace Identidade.Publico.Commands
{
    public class HealthCheckIdentityCommand
    {
        public Guid Id { get; }

        public HealthCheckIdentityCommand(Guid id)
        {
            Id = id;
        }
    }
}
