namespace ContaCorrente.Infrastructure.Messaging
{
    public static class KafkaTopics
    {
        public const string TransferenciasSolicitadas = "transferencias-solicitadas";
        public const string TransferenciasResultado = "transferencias-resultado";
        public const string TarifacoesRealizadas = "tarifacoes-realizadas";
        public const string UsuarioCadastrado = "usuario-cadastrado";
        public const string UsuarioInativado = "usuario-inativado";
        public const string DepositoSolicitado = "deposito-solicitado";
        public const string SaqueSolicitado = "saque-solicitado";
        public const string DepositoResultado = "deposito-resultado";
        public const string SaqueResultado = "saque-resultado";
    }
}
