namespace Autenticacao.Infrastructure.Messaging.Messages
{
    public class UsuarioInativadoMessage
    {
        public int IdUsuario { get; set; }
        public string NumeroConta { get; set; } = string.Empty;
        public DateTime DataInativacao { get; set; }
    }
}
