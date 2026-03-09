namespace Caixa.Domain.Entities
{
    public class Deposito
    {
        public int Id { get; set; }
        public string IdRequisicao { get; set; }
        public string NumeroConta { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataDeposito { get; set; }
        public string Status { get; set; } // PENDENTE, SUCESSO, ERRO
        public string? MensagemErro { get; set; }
    }
}
