namespace Caixa.Infrastructure.Messaging.Messages
{
    public class SaqueResultadoMessage
    {
        public string IdRequisicao { get; set; } = string.Empty;
        public bool Sucesso { get; set; }
        public string? MensagemErro { get; set; }
        public DateTime DataProcessamento { get; set; }
    }
}
