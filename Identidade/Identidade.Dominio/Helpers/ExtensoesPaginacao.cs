using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Identidade.Dominio.Helpers
{
    public static class ExtensoesPaginacao
    {
        public static async Task<ResultadoPaginado<T>> ParaResultadoPaginado<T>(
            this IQueryable<T> query,
            OpcoesPaginacao opcoesPaginacao,
            CancellationToken cancellationToken = default)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            if (opcoesPaginacao == null) throw new ArgumentNullException(nameof(opcoesPaginacao));

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip(opcoesPaginacao.Skip)
                .Take(opcoesPaginacao.TamanhoPagina)
                .ToArrayAsync(cancellationToken);

            return new ResultadoPaginado<T>
            {
                Items = items,
                Pagina = opcoesPaginacao.Pagina,
                TamanhoPagina = opcoesPaginacao.TamanhoPagina,
                Total = totalCount
            };
        }
    }
}
