namespace Tarifa.Infrastructure.Messaging.Messages
{
    public class TarifacaoRealizadaMessage
    {
        public string NumeroContaCorrente { get; set; } = string.Empty;
        public decimal ValorTarifa { get; set; }
        public string IdRequisicaoOrigem { get; set; } = string.Empty;
        public DateTime DataTarifacao { get; set; }
    }
}
