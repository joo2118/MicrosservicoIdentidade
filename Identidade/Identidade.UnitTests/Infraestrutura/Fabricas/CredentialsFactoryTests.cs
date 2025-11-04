#nullable enable

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Identidade.Infraestrutura.Factory;
using Xunit;
using Identidade.Dominio.Modelos;

namespace Identidade.UnitTests.Infraestrutura.Factory
{
    public class CredentialsFactoryTests
    {
        [Fact]
        public void Create_ShouldThrowArgumentException_WhenAuthorizationTokenIsInvalid()
        {
            var configuration = Substitute.For<IConfiguration>();
            var credentialsFactory = new CredentialsFactory();
            var invalidToken = "InvalidToken";

            Assert.Throws<ArgumentException>(() => credentialsFactory.Create(invalidToken));
        }

        [Fact]
        public void Create_ShouldThrowInvalidOperationException_WhenTokenVersionIsInvalid()
        {
            var configuration = Substitute.For<IConfiguration>();
            var credentialsFactory = new CredentialsFactory();
            var token = GenerateJwtToken(Array.Empty<Claim>());

            Assert.Throws<InvalidOperationException>(() => credentialsFactory.Create(token));
        }

        [Fact]
        public void Create_ShouldThrowInvalidOperationException_WhenValidUserEmailClaimIsInvalid()
        {
            var configuration = Substitute.For<IConfiguration>();
            var credentialsFactory = new CredentialsFactory();
            var claims = new Claim[]
            {
                new Claim(Constants.Token.Claim.Type.ver, Constants.Token.Claim.Value.v1),
                new Claim(Constants.Token.Claim.Type.unique_name, "user@testexample.com")
            };

            var token = GenerateJwtToken(claims);

            Assert.Throws<InvalidOperationException>(() => credentialsFactory.Create(token));
        }

        [Fact]
        public void Create_ShouldReturnCredentials_WhenValidUserEmail_v2_Claim()
        {
            var configuration = Substitute.For<IConfiguration>();
            var credentialsFactory = new CredentialsFactory();
            var claims = new Claim[]
            {
                new Claim(Constants.Token.Claim.Type.ver, "v2"),
                new Claim(Constants.Token.Claim.Type.preferred_username, "user@acp200394.onmicrosoft.com")
            };
            var token = GenerateJwtToken(claims);

            var result = credentialsFactory.Create(token);

            Assert.NotNull(result);
            Assert.Equal("user", result.UserLogin);
        }

        [Fact]
        public void Create_ShouldReturnCredentials_WhenValidAppIdClaim()
        {
            var configuration = Substitute.For<IConfiguration>();
            var credentialsFactory = new CredentialsFactory();
            var claims = new Claim[]
            {
                new Claim(Constants.Token.Claim.Type.ver, Constants.Token.Claim.Value.v1),
                new Claim(Constants.Token.Claim.Type.appid, "appId"),
                new Claim(Constants.Token.Claim.Type.appname, "appName")
            };
            var token = GenerateJwtToken(claims);

            var result = credentialsFactory.Create(token);

            Assert.NotNull(result);
            Assert.Equal("appId", result.MultitenantId);
            Assert.Equal("appName", result.UserLogin);
        }

        private static string GenerateJwtToken(Claim[] claims, string scheme = "Bearer")
        {
            var header = new JwtHeader();
            header.Add(JwtHeaderParameterNames.Alg, "HS256");
            var payload = new JwtPayload();
            payload.AddClaims(claims);

            var jwtToken = new JwtSecurityToken(header, payload);
            var handler = new JwtSecurityTokenHandler();
            return $"{scheme} {handler.WriteToken(jwtToken)}";
        }
    }
}
