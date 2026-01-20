using System;
using System.Collections.Generic;
using System.Linq;
using Identidade.Dominio.Interfaces;
using Identidade.Publico.Enumerations;

namespace Identidade.Infraestrutura.Helpers
{
    internal class PermissaoOperacaoHelper : IPermissaoOperacaoHelper
    {
        public string[] GetOperacoes(int somaOperacoes) => GetOperacoesStatic(somaOperacoes);

        public int GetSomaOperacoes(IEnumerable<string> operacoes) => GetSomaOperacoesStatic(operacoes);

        public static string[] GetOperacoesStatic(int somaOperacoes)
        {
            somaOperacoes = Math.Min(somaOperacoes, (int)PermissionOperation.All);
            var operacoes = new List<string>();

            var operacoesValores = Enum.GetValues(typeof(PermissionOperation))
                .Cast<int>()
                .OrderByDescending(i => i);

            foreach (int operation in operacoesValores)
            {
                if (somaOperacoes - operation < 0)
                    continue;

                operacoes.Add(((PermissionOperation)operation).ToString());
                somaOperacoes -= operation;
            }

            return operacoes.ToArray();
        }

        public static int GetSomaOperacoesStatic(IEnumerable<string> operacoes) =>
            Math.Min(
                operacoes.Sum(operation => (int)Enum.Parse(typeof(PermissionOperation), operation)),
                (int)PermissionOperation.All);
    }
}
