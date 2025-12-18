using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

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
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                          .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                          .AddEnvironmentVariables();
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders();
                    
                    var connectionString = context.Configuration["ApplicationInsights:ConnectionString"];
                    
                    var loggerConfig = new LoggerConfiguration()
                        .MinimumLevel.Information()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .MinimumLevel.Override("System", LogEventLevel.Warning)
                        .Enrich.FromLogContext()
                        .Enrich.WithProperty("Application", "Identidade.RESTAPI")
                        .WriteTo.Console();
                    
                    if (!string.IsNullOrEmpty(connectionString))
                    {
                        loggerConfig.WriteTo.ApplicationInsights(
                            connectionString,
                            TelemetryConverter.Traces);
                    }
                    
                    Log.Logger = loggerConfig.CreateLogger();
                    logging.AddSerilog(Log.Logger);
                })
                .ConfigureServices((context, services) =>
                {
                    var connectionString = context.Configuration["ApplicationInsights:ConnectionString"];
                    
                    if (!string.IsNullOrEmpty(connectionString))
                    {
                        services.AddApplicationInsightsTelemetry(options =>
                        {
                            options.ConnectionString = connectionString;
                            options.EnableAdaptiveSampling = true;
                            options.EnableQuickPulseMetricStream = true;
                        });
                    }
                })
                .UseStartup<Startup>();
    }
}