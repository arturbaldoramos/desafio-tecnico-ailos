namespace Tarifa.Domain.Entities
{
    public class Tarifacao
    {
        public int IdTarifacao { get; set; }
        public string NumeroContaCorrente { get; set; } = string.Empty;
        public string IdRequisicaoTransferencia { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime DataTarifacao { get; set; }
    }
}
