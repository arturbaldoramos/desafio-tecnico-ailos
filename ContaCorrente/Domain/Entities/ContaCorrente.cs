namespace ContaCorrente.Domain.Entities
{
    public class ContaCorrente
    {
        public int Id{ get; set; }
        public string Numero { get; set; }
        public string Nome { get; set; }
        public string Cpf { get; set; }
        public int Ativo { get; set; } // 1 para Ativa, 0 para Inativa
    }
}
