namespace Caixa.Application.DTOs
{
    public record SaqueResponse(string IdRequisicao, string NumeroConta, decimal Valor, string Status, DateTime Data);
}
