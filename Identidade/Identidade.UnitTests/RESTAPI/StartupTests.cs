using Identidade.RESTAPI;
using NSubstitute;
using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Identidade.Infraestrutura.Data;
using Microsoft.EntityFrameworkCore;
using Identidade.Dominio.Servicos;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Converters;
using Microsoft.AspNetCore.Http;
using System;
using Identidade.Dominio.Modelos;

namespace Identidade.UnitTests.RESTAPI
{
    public class StartupTests
    {
        [Fact]
        public void ConstructorTest()
        {
            var inMemorySettings = new Dictionary<string, string>
                    {
                        {"HealthCheck:MaxMemory", "1024"},
                        {"JWT:Issuer", "testIssuer"},
                        {"JWT:Audience", "testAudience"},
                        {"Redis.Url", "localhost"},
                        {"SharedCache.Redis.DefaultExpire", "00:30:00"},
                        {"Redis.SetAliveInterval", "00:00:10"}
                    };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
            var startup = new Startup(configuration);
            Assert.NotNull(startup);
        }

        [Fact]
        public void ConfigureServicesTest()
        {
            var inMemorySettings = new Dictionary<string, string>
                        {
                            {"HealthCheck:MaxMemory", "1024"},
                            {"JWT:Issuer", "testIssuer"},
                            {"JWT:Audience", "testAudience"},
                            {"Redis.Url", "localhost"},
                            {"SharedCache.Redis.DefaultExpire", "00:30:00"},
                            {"Redis.SetAliveInterval", "00:00:10"},
                            {"ConnectionStrings:DefaultConnection", "5H/j4tXBnB04nbtu9ql4OKcLrMqja5VHZg9OJxY7vMo="},
                            {"AzureAd:Instance", "https://login.microsoftonline.com/"},
                            {"AzureAd:Domain", "yourdomain.com"},
                            {"AzureAd:TenantId", "your-tenant-id"},
                            {"AzureAd:ClientId", "your-client-id"}
                        };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
            var startup = new Startup(configuration);

            var services = new ServiceCollection();

            startup.ConfigureServices(services);

            var sp = services.BuildServiceProvider();
            
            var healthCheckService = sp.GetService<HealthCheckService>();
            Assert.NotNull(healthCheckService);
            
            Assert.NotNull(sp.GetService<IAuthorizationService>());

            var options = sp.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get(JwtBearerDefaults.AuthenticationScheme);
            Assert.True(options.TokenValidationParameters.ValidateAudience);
            Assert.Equal("testAudience", options.TokenValidationParameters.ValidAudience);
            Assert.True(options.TokenValidationParameters.ValidateIssuer);
            Assert.Equal("testIssuer", options.TokenValidationParameters.ValidIssuer);
            Assert.True(options.TokenValidationParameters.ValidateLifetime);
            Assert.True(options.TokenValidationParameters.RequireSignedTokens);
            Assert.Contains("RS256", options.TokenValidationParameters.ValidAlgorithms);
            Assert.True(options.TokenValidationParameters.ValidateIssuerSigningKey);

            var authOptions = sp.GetRequiredService<IOptionsMonitor<AuthenticationOptions>>().CurrentValue;
            Assert.Equal(JwtBearerDefaults.AuthenticationScheme, authOptions.DefaultAuthenticateScheme);
            Assert.Equal(JwtBearerDefaults.AuthenticationScheme, authOptions.DefaultChallengeScheme);
            Assert.Equal(JwtBearerDefaults.AuthenticationScheme, authOptions.DefaultScheme);

            var swaggerOptions = sp.GetRequiredService<IOptions<SwaggerGenOptions>>().Value;
            Assert.Contains(swaggerOptions.SwaggerGeneratorOptions.SwaggerDocs, doc => doc.Key == "v1" && doc.Value.Title == "Identidade API" && doc.Value.Version == "v1");

            var jsonOptions = sp.GetRequiredService<IOptions<MvcNewtonsoftJsonOptions>>().Value;
            Assert.Contains(jsonOptions.SerializerSettings.Converters, converter => converter is StringEnumConverter);
        }

        [Fact]
        public void ConfigureTest()
        {
            string connectionString = "Data Source=serverName;Initial Catalog=databaseName;User ID=username;Password=password;";

            var inMemorySettings = new Dictionary<string, string>
                    {
                        {"HealthCheck:MaxMemory", "1024"},
                        {"JWT:Issuer", "testIssuer"},
                        {"JWT:Audience", "testAudience"},
                        {"Redis.Url", "localhost"},
                        {"SharedCache.Redis.DefaultExpire", "00:30:00"},
                        {"Redis.SetAliveInterval", "00:00:10"}
                    };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
            var startup = new Startup(configuration);

            var services = new ServiceCollection();
            var app = Substitute.For<IApplicationBuilder>();
            var scopeFactory = Substitute.For<IServiceScopeFactory>();
            var env = Substitute.For<IWebHostEnvironment>();

            var serviceColletion = new ServiceCollection();

            services.AddSingleton(scopeFactory);
            services.AddDbContext<ARCDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddRouting();
            services.AddSwaggerGen();
            services.AddSwaggerGenNewtonsoftSupport();
            services.AddAuthorization();

            var serviceProvider = services.BuildServiceProvider();

            app.ApplicationServices.Returns(serviceProvider);

            env.EnvironmentName.Returns(Constants.cst_UnitTests);
            env.WebRootPath.Returns((string)null);

            startup.Configure(app, env);

            app.Received().UseHttpsRedirection();
            app.Received().UseAuthentication();
            app.Received().UseAuthentication();
            app.Received(12).Use(Arg.Any<Func<RequestDelegate, RequestDelegate>>());
        }
    }
}