#nullable enable

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Linq;
using Identidade.Dominio.Modelos;
using Identidade.Dominio.Interfaces;
using System.Collections.Generic;

namespace Identidade.Infraestrutura.Fabricas
{
    public class CredentialsFactory : ICredentialsFactory
    {
        public Credentials Create(string authorizationToken, string? requestUser = null)
        {
            var claims = GetTokenClaims(authorizationToken);
            ValidateTokenVersion(claims);

            var userEmailClaim = GetUserEmailClaim(claims);
            if (!claims.TryGetValue(userEmailClaim, out string? strMail) || strMail is null)
                return HandleAppIdClaims(claims, requestUser);

            return HandleUserEmailClaims(strMail);
        }

        private static void ValidateTokenVersion(Dictionary<string, string> claims)
        {
            if (!claims.TryGetValue(Constants.Token.Claim.Type.ver, out string? version) || string.IsNullOrWhiteSpace(version))
                throw new InvalidOperationException("Invalid JWT Authorization Token.");
        }

        private static string GetUserEmailClaim(Dictionary<string, string> claims)
        {
            var version = claims[Constants.Token.Claim.Type.ver];
            return version.Equals(Constants.Token.Claim.Value.v1)
                ? Constants.Token.Claim.Type.unique_name
                : Constants.Token.Claim.Type.preferred_username;
        }

        private static Credentials HandleAppIdClaims(Dictionary<string, string> claims, string? requestUser)
        {
            if (claims.TryGetValue(Constants.Token.Claim.Type.appid, out string? appId))
            {
                if (claims.TryGetValue(Constants.Token.Claim.Type.appname, out string? appName))
                    return new Credentials(appId, appName);
                else
                    return new Credentials(appId, appId);
            }
            else
            {
                throw new InvalidOperationException("Invalid JWT Authorization Token.");
            }
        }

        private static Credentials HandleUserEmailClaims(string strMail)
        {
            if (!TryGetMailAddress(strMail, out MailAddress? userEmail) || userEmail is null)
                throw new InvalidOperationException("Invalid JWT Authorization Token.");

            return CreateCredentials(userEmail);
        }

        private static Dictionary<string, string> GetTokenClaims(string authorizationToken)
        {
            if (!AuthenticationHeaderValue.TryParse(authorizationToken, out var headerValue)
                || !headerValue.Scheme.Equals(Constants.Token.Scheme.Bearer)
                || string.IsNullOrWhiteSpace(headerValue.Parameter))
                throw new ArgumentException("Invalid JWT Authorization Token or Header.");

            var jsonToken = new JwtSecurityTokenHandler().ReadJwtToken(headerValue.Parameter);

            return jsonToken.Claims
                .GroupBy(c => c.Type)
                .ToDictionary(g => g.Key, g => g.First().Value);
        }

        private static Credentials CreateCredentials(MailAddress userEmail)
        {
            var userPattern = userEmail.User.Split("-");
            if (userPattern.Length < 2)
                throw new InvalidOperationException("Invalid JWT Authorization Token.");

            return new Credentials(userPattern[0], userPattern.Skip(1).Aggregate((a, b) => $"{a}-{b}"));
        }

        private static bool TryGetMailAddress(string userEmail, out MailAddress? mailAddress)
        {
            try
            {
                mailAddress = new MailAddress(userEmail);
                return true;
            }
            catch
            {
                mailAddress = null;
                return false;
            }
        }
    }
}
