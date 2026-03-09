namespace Caixa.Application.DTOs
{
    public record DepositoResponse(string IdRequisicao, string NumeroConta, decimal Valor, string Status, DateTime Data);
}
