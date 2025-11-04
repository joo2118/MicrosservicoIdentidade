using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Identidade.RESTAPI
{
    /// <summary>
    /// Classe base do Programa da RESTAPI.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Função principal da aplicação, chamada na inicialização.
        /// </summary>
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Classe de Builder que cria o Web Host da aplicação.
        /// </summary>
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .UseBeatPulse(options =>
            {
                options.ConfigurePath(path: "health")
                    .ConfigureTimeout(milliseconds: 1500)
                    .ConfigureDetailedOutput(detailedOutput: true, includeExceptionMessages: true);
            })
            .UseStartup<Startup>();
    }
}
