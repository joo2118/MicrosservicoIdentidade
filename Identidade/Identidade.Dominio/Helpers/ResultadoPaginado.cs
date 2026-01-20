using System.Collections.Generic;

namespace Identidade.Dominio.Helpers
{
    public sealed class ResultadoPaginado<T>
    {
        public required IReadOnlyCollection<T> Items { get; init; }
        public required int Pagina { get; init; }
        public required int TamanhoPagina { get; init; }
        public required int Total { get; init; }
    }
}
