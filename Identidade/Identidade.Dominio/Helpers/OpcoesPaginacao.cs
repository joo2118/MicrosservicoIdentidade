namespace Identidade.Dominio.Helpers
{
    public sealed class OpcoesPaginacao
    {
        public int Pagina { get; }
        public int TamanhoPagina { get; }

        public OpcoesPaginacao(int? pagina, int? tamanhoPagina, int tamanhoDefault = 50, int tamanhoMaximo = 200)
        {
            var p = pagina ?? 1;
            if (p < 1) p = 1;

            var ps = tamanhoPagina ?? tamanhoDefault;
            if (ps < 1) ps = tamanhoDefault;
            if (ps > tamanhoMaximo) ps = tamanhoMaximo;

            Pagina = p;
            TamanhoPagina = ps;
        }

        public int Skip => (Pagina - 1) * TamanhoPagina;
    }
}
