using Identidade.Infraestrutura.Interfaces;
using System;
using System.Linq;
using Xunit;
using NSubstitute;
using Microsoft.Extensions.Configuration;
using Identidade.Infraestrutura.Servicos;

namespace Identidade.UnitTests.Infraestrutura.Services
{
    public class HealthCheckServiceTests
    {
        [Fact]
        public void Execute_Test()
        {
            var processId = 1234;
            var processName = "TestProcessName";
            var clientId = "TestClientId";

            var environment = Substitute.For<IEnvironmentAdapter>();

            environment.ProcessId.Returns(processId);
            environment.CommandLine.Returns(processName);

            var configuration = Substitute.For<IConfiguration>();

            var clientIdSection = Substitute.For<IConfigurationSection>();

            clientIdSection.Value.Returns(clientId);


            var service = new HealthCheckService(environment, configuration);
            var actual = service.Execute();

            Assert.NotNull(actual);
            Assert.Equal(processId.ToString(), actual.ProcessId);
            Assert.Equal(processName, actual.ProcessName);

            Assert.Empty(actual.ConfigItems);
        }

        [Fact]
        public void Execute_InvalidProcessName_Test()
        {
            var service = new HealthCheckService(Substitute.For<IEnvironmentAdapter>(), Substitute.For<IConfiguration>());
            var ex = Assert.Throws<InvalidOperationException>(() => service.Execute());

            Assert.NotNull(ex);
            Assert.Equal("Could not determine the executable process name from the command line.", ex.Message);
        }
    }
}
