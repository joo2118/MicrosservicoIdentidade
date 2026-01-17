using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Identidade.Infraestrutura.Data;
using Serilog.Events;
using System;
using Xunit;
using Identidade.Infraestrutura.Configuracoes;

namespace Identidade.UnitTests
{
    public class SharedConfigurationTests
    {
        [Fact]
        public void ConfigureWithServiceCollectionNull()
        {
            var settingsMoq = new Mock<ISettings>();

            Assert.Throws<ArgumentNullException>(() => SharedConfiguration.Configure(null, settingsMoq.Object));
        }

        [Fact]
        public void ConfigureWithSettingsNull()
        {
            var serviceCollectionMoq = new Mock<IServiceCollection>();

            Assert.Throws<NullReferenceException>(() => SharedConfiguration.Configure(serviceCollectionMoq.Object, null));
        }

        [Fact]
        public void ConfigureTest()
        {
            var settingsMoq = new Mock<ISettings>();
            IServiceCollection services = new ServiceCollection();

            var connec = new ConnectionStrings("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=Texto Plano;");

            settingsMoq.Setup(s => s.ConnectionStrings).Returns(connec);

            SharedConfiguration.Configure(services, settingsMoq.Object);

            Assert.NotNull(services);
        }

        [Fact]
        public void UpdateDatabaseInMemoryTest()
        {
            ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped<ARCDbContext>();
            serviceCollection.AddDbContext<ARCDbContext>(op => op.UseInMemoryDatabase("database"));
         
            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            
            SharedConfiguration.UpdateDatabase(serviceProvider, "Tests");

            Assert.NotNull(serviceProvider);
        }

        [Fact]
        public void CreateLoggerTest()
        {
            var settingsMoq = new Mock<ISettings>();
            var log = new Logging(true, "", "http://Teste", true, LogEventLevel.Information);

            settingsMoq.Setup(s => s.Logging).Returns(log);
            SharedConfiguration.CreateLogger(settingsMoq.Object);


            Assert.NotNull(settingsMoq);
            Assert.NotNull(log);
        }
    }
}
