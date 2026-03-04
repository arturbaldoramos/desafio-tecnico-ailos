namespace ContaCorrente.Domain.Interfaces
{
    public interface ISaldoCacheService
    {
        Task<decimal?> ObterSaldoAsync(string numeroConta);
        Task DefinirSaldoAsync(string numeroConta, decimal saldo);
        void InvalidarSaldo(string numeroConta);
    }
}
