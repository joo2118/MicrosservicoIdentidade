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
            app.Received(13).Use(Arg.Any<Func<RequestDelegate, RequestDelegate>>());
        }
    }
}