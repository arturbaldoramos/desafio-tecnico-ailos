namespace Caixa.Application.DTOs
{
    public record DepositoRequest(
        string IdRequisicao,
        decimal Valor
    );
}
