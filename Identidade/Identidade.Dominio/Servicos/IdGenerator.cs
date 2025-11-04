using System;

namespace Identidade.Dominio.Servicos
{
    public interface IIdGenerator
    {
        string GenerateId(string prefix, string suggestedId = null);
    }

    public class IdGenerator : IIdGenerator
    {
        public string GenerateId(string prefix, string suggestedId = null) =>
            (suggestedId?.StartsWith(prefix) ?? false)
                ? suggestedId
                : prefix + "_" + Guid.NewGuid();
    }
}
