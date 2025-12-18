using Identidade.Infraestrutura.Configuracoes;
using Identidade.Infraestrutura.Configuracoes.ServiceBus;
using Identidade.Infraestrutura.RedisNotifier;
using Identidade.RESTAPI.Configurations;
using Identidade.RESTAPI.Middleware;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Serilog;
using System;
using System.IO;
using System.Timers;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;
using System.Linq;

namespace Identidade.RESTAPI
{
    /// <summary>
    /// Classe de Startup para a configuração da aplicação Identidade REST API.
    /// </summary>
    public class Startup
    {
        private readonly RestApiSettings _settings;
        private readonly IConfiguration _configuration;
        private readonly RedisStatusNotifier _statusNotifier;

        /// <summary>
        /// Inicializa uma nova instância da Classe <see cref="Startup"/>.
        /// </summary>
        /// <param name="configuration">A configuração da aplicação.</param>
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
            _settings = new RestApiSettings(configuration);
            _statusNotifier = new RedisStatusNotifier(new Timer(_settings.RedisSetAliveInterval), new ConnectionMultiplexerProxy(_settings, _settings.RedisUrl), RedisConstants.Path.REDIS_IDENTITYRESTAPI, RedisConstants.Field.REDIS_FIELD_IDENTITYRESTAPIID);
        }

        /// <summary>
        /// Configura os serviços da aplicação. Metodo chamado durante runtime.
        /// </summary>
        /// <param name="services">A coleção de serviços para configurar.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            _statusNotifier.SetStarting();
            services.AddMvc();
            services
                .AddControllers()
                .AddNewtonsoftJson(op =>
                    op.SerializerSettings.Converters.Add(new StringEnumConverter())
                 );

            services.AddTransient<ISettings, RestApiSettings>();

            services.AddSingleton(_statusNotifier);
           

            SharedConfiguration.Configure(services, _settings);

            services.AddMassTransit(x =>
            {
                ConfiguradorServiceBus.Configure(x, _settings, ConfiguradorEndpoints.Empty);
            });

            services.AddSingleton(c => SharedConfiguration.CreateLogger(_settings));

            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description =
                    "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    }, Array.Empty<string>()
                } });
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Identidade API", Version = "v1" });
            });
            services.AddSwaggerGenNewtonsoftSupport();

            // Modern ASP.NET Core Health Checks replacing BeatPulse
            services.AddHealthChecks()
                .AddSqlServer(
                    connectionString: _settings.ConnectionStrings.DefaultConnection,
                    healthQuery: "SELECT 1;",
                    name: "sql",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new string[] { "db", "sql", "sqlserver" })
                .AddPrivateMemoryHealthCheck(
                    maximumMemoryBytes: _settings.HealthCheck.MaxMemory * 1024L * 1024L, // Convert MB to bytes
                    name: "private_memory",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new string[] { "memory" });

            _statusNotifier.SetIdle();
        }
        /// <summary>
        /// Configura a pipeline de chamadas HTTP. Método chamado durante runtime.
        /// </summary>
        /// <param name="app">O Builder da aplicação.</param>
        /// <param name="env">O ambiente de host web.</param>

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Log.Logger = SharedConfiguration.CreateLogger(_settings);
            SharedConfiguration.UpdateDatabase(app.ApplicationServices, env.EnvironmentName);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseMiddleware<PerformanceMetricsMiddleware>();

            app.UseHttpsRedirection();
            app.UseCors();
            app.UseSwagger();

            var webRootPath = env.WebRootPath ?? Path.GetFullPath(Path.Combine("..", "..", "..", "..", "Identidade.RESTAPI", "wwwroot"));
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(webRootPath, "css")),
                RequestPath = "/css"
            });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "Identidade API V1");
                c.InjectStylesheet("/css/identidade-swagger-v1.css");
            });

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            if (!env.IsEnvironment(Dominio.Modelos.Constants.cst_UnitTests))
            {
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapHealthChecks("/health", new HealthCheckOptions
                    {
                        ResponseWriter = async (context, report) =>
                        {
                            context.Response.ContentType = "application/json";
                            
                            var result = JsonSerializer.Serialize(new
                            {
                                status = report.Status.ToString(),
                                duration = report.TotalDuration,
                                checks = report.Entries.Select(e => new
                                {
                                    name = e.Key,
                                    status = e.Value.Status.ToString(),
                                    duration = e.Value.Duration,
                                    description = e.Value.Description,
                                    exception = e.Value.Exception?.Message,
                                    data = e.Value.Data
                                })
                            }, new JsonSerializerOptions
                            {
                                WriteIndented = true
                            });
                            
                            await context.Response.WriteAsync(result);
                        },
                        AllowCachingResponses = false
                    });
                    
                    endpoints.MapDefaultControllerRoute();
                    endpoints.MapControllerRoute("users", "{controller=Users}");
                    endpoints.MapControllerRoute("groups", "{controller=groups}");
                });
            }
        }
    }
}