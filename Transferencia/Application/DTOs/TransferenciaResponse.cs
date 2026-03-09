namespace Transferencia.Application.DTOs
{
    public record TransferenciaResponse(
        int Id,
        string ContaOrigem,
        string ContaDestino,
        decimal Valor,
        DateTime DataMovimento,
        string Status
    );
}
