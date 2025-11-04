using System;
using Xunit;

namespace Identidade.Publico.Commands
{
    public class HealthCheckIdentityCommandTests
    {
        [Fact]
        public void Constructor_SetsIdProperty()
        {
            var id = Guid.NewGuid();
            var command = new HealthCheckIdentityCommand(id);

            Assert.Equal(id, command.Id);
        }
    }
}
