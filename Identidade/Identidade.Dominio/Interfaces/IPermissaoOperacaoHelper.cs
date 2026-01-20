using System.Collections.Generic;

namespace Identidade.Dominio.Interfaces
{
    public interface IPermissaoOperacaoHelper
    {
        string[] GetOperacoes(int somaOperacoes);
        int GetSomaOperacoes(IEnumerable<string> operacoes);
    }
}
