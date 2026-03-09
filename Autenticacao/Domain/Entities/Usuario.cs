namespace Autenticacao.Domain.Entities
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Cpf { get; set; }
        public string Nome { get; set; }
        public string SenhaHash { get; set; }
        public string NumeroConta { get; set; }
        public int Ativo { get; set; }
        public DateTime DataCriacao { get; set; }
    }
}
