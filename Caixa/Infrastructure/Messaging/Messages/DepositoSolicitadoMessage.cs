namespace Caixa.Infrastructure.Messaging.Messages
{
    public class DepositoSolicitadoMessage
    {
        public string IdRequisicao { get; set; } = string.Empty;
        public string NumeroConta { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime DataSolicitacao { get; set; }
    }
}
