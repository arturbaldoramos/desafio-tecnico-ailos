namespace Transferencia.Infrastructure.Messaging.Messages
{
    public class TransferenciaSolicitadaMessage
    {
        public string IdRequisicao { get; set; } = string.Empty;
        public string NumeroContaOrigem { get; set; } = string.Empty;
        public string NumeroContaDestino { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime DataSolicitacao { get; set; }
    }
}
