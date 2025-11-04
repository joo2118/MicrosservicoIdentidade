using System;

namespace Identidade.Dominio.Modelos
{
    public class Credentials
    {
        public Credentials(string multitenantId, string userLogin)
        {
            if (string.IsNullOrWhiteSpace(multitenantId))
                throw new ArgumentException($"'{nameof(multitenantId)}' cannot be null or whitespace.", nameof(multitenantId));

            if (string.IsNullOrWhiteSpace(userLogin))
                throw new ArgumentException($"'{nameof(userLogin)}' cannot be null or whitespace.", nameof(userLogin));

            MultitenantId = multitenantId;
            UserLogin = userLogin;
        }

        public string MultitenantId { get; }
        public string UserLogin { get; }
    }
}
