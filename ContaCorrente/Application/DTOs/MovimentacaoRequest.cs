namespace ContaCorrente.Application.DTOs
{
    public record MovimentacaoRequest(
        string IdRequisicao,
        string Tipo, // "D" para Débito, "C" para Crédito
        decimal Valor
    );
}
