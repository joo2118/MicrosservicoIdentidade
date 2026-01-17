#nullable enable

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Identidade.Dominio.Modelos;
using Xunit;

namespace Identidade.UnitTests.Infraestrutura.Fabricas
{
    public class CredentialsFactoryTests
    {
        [Fact]
        public void Create_ShouldThrowArgumentException_WhenAuthorizationTokenIsInvalid()
        {
            var credentialsFactory = new Identidade.Infraestrutura.Fabricas.CredentialsFactory();
            var invalidToken = "InvalidToken";

            Assert.Throws<ArgumentException>(() => credentialsFactory.Create(invalidToken));
        }

        [Fact]
        public void Create_ShouldThrowInvalidOperationException_WhenTokenVersionIsInvalid()
        {
            var credentialsFactory = new Identidade.Infraestrutura.Fabricas.CredentialsFactory();
            var token = GenerateJwtToken(Array.Empty<Claim>());

            Assert.Throws<InvalidOperationException>(() => credentialsFactory.Create(token));
        }

        [Fact]
        public void Create_ShouldThrowInvalidOperationException_WhenValidUserEmailClaimIsInvalid()
        {
            var credentialsFactory = new Identidade.Infraestrutura.Fabricas.CredentialsFactory();
            var claims = new[]
            {
                new Claim(Constants.Token.Claim.Type.ver, Constants.Token.Claim.Value.v1),
                new Claim(Constants.Token.Claim.Type.unique_name, "user@testexample.com")
            };

            var token = GenerateJwtToken(claims);

            Assert.Throws<InvalidOperationException>(() => credentialsFactory.Create(token));
        }

        [Fact]
        public void Create_ShouldReturnCredentials_WhenValidAppIdClaim()
        {
            var credentialsFactory = new Identidade.Infraestrutura.Fabricas.CredentialsFactory();
            var claims = new[]
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
