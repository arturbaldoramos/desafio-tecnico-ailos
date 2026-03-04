namespace Transferencia.Application.DTOs
{
    public record TransferenciaRequest(
        string IdRequisicao,
        string ContaDestino,
        decimal Valor
    );
}
