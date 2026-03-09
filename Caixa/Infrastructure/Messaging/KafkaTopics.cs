namespace Caixa.Infrastructure.Messaging
{
    public static class KafkaTopics
    {
        public const string DepositoSolicitado = "deposito-solicitado";
        public const string SaqueSolicitado = "saque-solicitado";
        public const string DepositoResultado = "deposito-resultado";
        public const string SaqueResultado = "saque-resultado";
    }
}
