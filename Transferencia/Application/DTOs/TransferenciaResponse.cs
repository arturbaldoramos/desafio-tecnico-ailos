namespace Transferencia.Application.DTOs
{
    public record TransferenciaResponse(
        int IdTransferencia,
        string ContaOrigem,
        string ContaDestino,
        decimal Valor,
        DateTime DataMovimento,
        string Status
    );
}
