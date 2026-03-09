namespace Autenticacao.Infrastructure.Messaging.Messages
{
    public class UsuarioCadastradoMessage
    {
        public int IdUsuario { get; set; }
        public string Cpf { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string NumeroConta { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
    }
}
