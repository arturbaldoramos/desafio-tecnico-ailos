namespace Transferencia.Infrastructure.Messaging.Messages
{
    public class TransferenciaRealizadaMessage
    {
        public string IdRequisicao { get; set; } = string.Empty;
        public string NumeroContaOrigem { get; set; } = string.Empty;
        public decimal ValorTransferencia { get; set; }
        public DateTime DataTransferencia { get; set; }
    }
}
