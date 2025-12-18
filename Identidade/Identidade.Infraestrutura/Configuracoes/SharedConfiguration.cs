using AutoMapper;
using Elastic.Ingest.Elasticsearch;
using Elastic.Serilog.Sinks;
using Identidade.Dominio.Helpers;
using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Modelos;
using Identidade.Dominio.Servicos;
using Identidade.Dominio.Writers;
using Identidade.Infraestrutura.Adaptadores;
using Identidade.Infraestrutura.ClientServices;
using Identidade.Infraestrutura.Data;
using Identidade.Infraestrutura.Factory;
using Identidade.Infraestrutura.Helpers;
using Identidade.Infraestrutura.Interfaces;
using Identidade.Infraestrutura.Services;
using Identidade.Infraestrutura.Servicos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Linq;
using ILogger = Serilog.ILogger;

namespace Identidade.Infraestrutura.Configuracoes
{
    public static class SharedConfiguration
    {
        public static void Configure(IServiceCollection services, ISettings settings)
        {
            BindServices(services);

            services.AddTransient(provider => new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile(provider.GetService<IReadOnlyRepository<User>>(), provider.GetService<IUserGroupRepository>()));
                cfg.AddProfile(new UserGroupProfile(provider.GetService<IReadOnlyRepository<UserGroup>>(), provider.GetService<IReadOnlyRepository<Permission>>(), provider.GetService<IPermissionOperationManager>()));
                cfg.AddProfile<PermissionProfile>();
                cfg.AllowNullCollections = true;
            }).CreateMapper());

            var connectionString = settings.ConnectionStrings.DefaultConnection;
            services.AddDbContext<ARCDbContext>(options =>
                options.UseSqlServer(connectionString,
                x => x.MigrationsHistoryTable(
                HistoryRepository.DefaultTableName,
                Constants.cst_SchemaIdentity
            )));

            services.AddIdentity<User, IdentityRole>()
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<ARCDbContext>();
            
            services.AddLogging(cfg => cfg.AddConsole().AddSerilog())
                .Configure<LoggerFilterOptions>(opt => opt.MinLevel = LogLevel.Information);
        }

        public static void UpdateDatabase(IServiceProvider provider, string env)
        {
            using (var serviceScope = provider
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<ARCDbContext>())
                {
                    if (env == Constants.cst_Tests)
                    {
                        context.Database.EnsureDeleted();
                        context.Database.EnsureCreated();
                    }
                    else if (env == Constants.cst_UnitTests)
                        return;
                    else
                    {
                        // Use Migrate() for Development and Production
                        // This will apply pending migrations without trying to create the database
                        context.Database.Migrate();
                    }

                    var permissionIds = context.Permissions.Select(p => p.Id).ToArray();
                    var remaindingPermissions = PermissionRecord.All.Where(p => !permissionIds.Contains(p.Id));
                    context.Permissions.AddRange(remaindingPermissions);
                    context.SaveChanges();
                }
            }
        }

        private static void BindServices(IServiceCollection services) =>
            services.AddScoped<IARCDbContext, ARCDbContext>()
                    .AddTransient<IReadOnlyRepository<User>, UserReadOnlyRepository>()
                    .AddTransient<IUserRepository, UserRepository>()
                    .AddTransient<ISignInManager, SignInManager>()
                    .AddTransient<ILogInService, EFLoginService>()
                    .AddTransient<IUserValidator, UserValidator>()
                    .AddTransient<IPasswordValidator, PasswordValidator>()
                    .AddTransient<IReadOnlyRepository<UserGroup>, UserGroupRepository>()
                    .AddTransient<IUserGroupRepository, UserGroupRepository>()
                    .AddTransient<IRepository<UserGroup>, UserGroupRepository>()
                    .AddTransient<IReadOnlyRepository<Permission>, PermissionRepository>()
                    .AddTransient<IUpdateConcurrencyResolver, UpdateConcurrencyResolver>()
                    .AddTransient<IAuthorizationService, AuthorizationService>()
                    .AddTransient<IIdGenerator, IdGenerator>()
                    .AddTransient<IPermissionOperationManager, PermissionOperationManager>()
                    .AddTransient<IPatchUserMerger, PatchUserMerger>()
                    .AddTransient<IUserClientService, UserClientService>()
                    .AddTransient<IUserGroupClientService, UserGroupClientService>()
                    .AddTransient<IPermissionClientService, PermissionClientService>()
                    .AddTransient<IAuthClientService, AuthClientService>()
                    .AddTransient<UserManager<User>>()
                    .AddTransient<SignInManager<User>>()
                    .AddScoped<ICredentialsFactory, CredentialsFactory>()
                    .AddScoped<IDatabaseConnectionUserModifier, DatabaseConnectionUserModifier>()
                    .AddScoped<IArcUserXmlWriter, ArcUserXmlWriter>()
                    .AddScoped<IHealthCheckService, HealthCheckService>()
                    .AddScoped<IEnvironmentAdapter, EnvironmentAdapter>();

        public static ILogger CreateLogger(ISettings settings)
        {
            var loggerConfig = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithProcessId()
                .WriteTo.Console()
                .MinimumLevel.Is(settings.Logging.LogLevel);

            if (settings.Logging.WriteToFile)
                loggerConfig.WriteTo.File(
                    settings.Logging.FilePath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 31);

            if (settings.Logging.WriteToElasticSearch)
            {
                try
                {
                    loggerConfig.WriteTo.Elasticsearch(new[] { new Uri(settings.Logging.ElasticSearchAddress) }, opts =>
                    {
                        opts.MinimumLevel = settings.Logging.LogLevel;
                        opts.BootstrapMethod = BootstrapMethod.Silent; // Changed from Failure to Silent
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to configure Elasticsearch logging: {ex.Message}");
                    Console.WriteLine("Application will continue without Elasticsearch logging.");
                }
            }

            return loggerConfig.CreateLogger();
        }
    }
}
