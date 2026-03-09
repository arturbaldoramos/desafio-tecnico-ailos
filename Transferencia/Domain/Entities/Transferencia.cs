namespace Transferencia.Domain.Entities
{
    public class Transferencia
    {
        public int Id { get; set; }
        public string IdRequisicao { get; set; } = string.Empty;
        public string NumeroContaOrigem { get; set; } = string.Empty;
        public string NumeroContaDestino { get; set; } = string.Empty;
        public DateTime DataMovimento { get; set; }
        public decimal Valor { get; set; }
        public string Status { get; set; } = "PENDENTE";
        public string? MensagemErro { get; set; }
    }

    public static class TransferenciaStatus
    {
        public const string Pendente = "PENDENTE";
        public const string Processando = "PROCESSANDO";
        public const string Sucesso = "SUCESSO";
        public const string Erro = "ERRO";
    }
}
