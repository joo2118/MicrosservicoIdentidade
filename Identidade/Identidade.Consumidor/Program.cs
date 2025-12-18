using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Identidade.Consumidor.Consumidores;
using Identidade.Consumidor.Helpers;
using Identidade.Infraestrutura.RedisNotifier;
using Identidade.Dominio.Interfaces;
using Serilog;
using Serilog.Events;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using MassTransit;
using Identidade.Infraestrutura.Configuracoes.ServiceBus;
using Identidade.Infraestrutura.Configuracoes;

namespace Identidade.Consumidor
{
    internal class Program
    {
        private static IRedisSettings _settings;
        private static IRedisStatusNotifier _statusNotifier;
        private static string _environmentName;

        private static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        private static async Task MainAsync()
        {
            _environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environments.Production;

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(typeof(Program).Assembly.Location))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{_environmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            _settings = new ConsumerSettings(configuration);
            
            var connectionString = configuration["ApplicationInsights:ConnectionString"];
            
            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "Identidade.Consumidor")
                .WriteTo.Console();
            
            if (!string.IsNullOrEmpty(connectionString))
            {
                loggerConfig.WriteTo.ApplicationInsights(
                    connectionString,
                    TelemetryConverter.Traces);
            }
            
            Log.Logger = loggerConfig.CreateLogger();

            _statusNotifier = new RedisStatusNotifier(new Timer(_settings.RedisSetAliveInterval), new ConnectionMultiplexerProxy(_settings, _settings.RedisUrl), RedisConstants.Path.REDIS_IDENTITYCONSUMERS, RedisConstants.Field.REDIS_FIELD_IDENTITYCONSUMERID);
            _statusNotifier.SetStarting();

            var host = new HostBuilder()
                .UseEnvironment(_environmentName)
                .ConfigureHostConfiguration(cfg => cfg.AddConfiguration(configuration))
                .ConfigureServices((hostContext, services) =>
                {
                    var aiConnectionString = hostContext.Configuration["ApplicationInsights:ConnectionString"];
                    if (!string.IsNullOrEmpty(aiConnectionString))
                    {
                        services.AddApplicationInsightsTelemetryWorkerService(options =>
                        {
                            options.ConnectionString = aiConnectionString;
                            options.EnableAdaptiveSampling = true;
                            options.EnableQuickPulseMetricStream = true;
                        });
                    }
                    
                    ConfigureServices(services, hostContext.HostingEnvironment.EnvironmentName);
                })
                .ConfigureLogging(ConfigureLogging);

            if (_environmentName == Environments.Development)
                await host.RunConsoleAsync();
            else
                await host.Build().RunAsync();
        }

        private static void ConfigureServices(IServiceCollection services, string env)
        {
            services.AddTransient<ISettings, ConsumerSettings>()
                    .AddTransient<IRedisSettings, ConsumerSettings>()
                    .AddTransient<IMessageManager, MessageManager>()
                    .AddTransient<IConnectionMultiplexerProxy, ConnectionMultiplexerProxy>();
            services.AddSingleton(_statusNotifier);

            services.AddScoped<ConsumidorCriaOuAtualizaUsuario>();
            services.AddScoped<ConsumidorDeletaUsuario>();
            services.AddScoped<ConsumidorCriaOuAtualizaGrupoUsuario>();
            services.AddScoped<ConsumidorDeletaGrupoUsuario>();
            services.AddScoped<ConsumidorHealthCheck>();

            services.AddScoped<ConsumidorCriaOuAtualizaUsuario>();
            services.AddScoped<ConsumidorDeletaUsuario>();
            services.AddScoped<ConsumidorCriaOuAtualizaGrupoUsuario>();
            services.AddScoped<ConsumidorDeletaGrupoUsuario>();
            services.AddScoped<ConsumidorHealthCheck>();
            services.AddScoped(typeof(PipelineFilter<>));

            var servicesScope = services.BuildServiceProvider();

            services.AddMassTransit(x =>
            {
                x.AddConsumers(typeof(Program).Assembly);

                ConfiguradorServiceBus.Configure(x, _settings, new Configuracoes.ConfiguradorEndpoints());
            });

            SharedConfiguration.Configure(services, _settings);
            SharedConfiguration.UpdateDatabase(services.BuildServiceProvider(), env);

            _statusNotifier.SetIdle();
        }

        private static void ConfigureLogging(HostBuilderContext hostContext, ILoggingBuilder loggingBuilder)
        {
            if (_environmentName == "Tests")
                return;

            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog(Log.Logger, dispose: true);
        }
    }
}