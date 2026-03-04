namespace Transferencia.Infrastructure.Messaging.Messages
{
    public class TransferenciaResultadoMessage
    {
        public string IdRequisicao { get; set; } = string.Empty;
        public bool Sucesso { get; set; }
        public string? MensagemErro { get; set; }
        public int? IdContaCorrenteDestino { get; set; }
        public DateTime DataProcessamento { get; set; }
    }
}
