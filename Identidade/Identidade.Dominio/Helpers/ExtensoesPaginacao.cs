using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

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

            int totalCount;
            T[] items;

            if (query.Provider is IAsyncQueryProvider)
            {
                totalCount = await query.CountAsync(cancellationToken);
                items = await query
                    .Skip(opcoesPaginacao.Skip)
                    .Take(opcoesPaginacao.TamanhoPagina)
                    .ToArrayAsync(cancellationToken);
            }
            else
            {
                totalCount = query.Count();
                items = query
                    .Skip(opcoesPaginacao.Skip)
                    .Take(opcoesPaginacao.TamanhoPagina)
                    .ToArray();
            }

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