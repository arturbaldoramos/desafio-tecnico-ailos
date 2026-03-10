namespace ContaCorrente.Application.DTOs
{
    public record ExtratoResponse(
        string NumeroConta,
        decimal SaldoAnterior,
        decimal SaldoFinal,
        int Pagina,
        int TamanhoPagina,
        int TotalRegistros,
        List<MovimentoExtratoItem> Movimentos);

    public record MovimentoExtratoItem(
        DateTime DataMovimento,
        string TipoMovimento,
        decimal Valor);
}
