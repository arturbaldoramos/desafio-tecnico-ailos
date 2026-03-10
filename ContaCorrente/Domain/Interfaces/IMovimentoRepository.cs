namespace ContaCorrente.Domain.Interfaces
{
    using ContaCorrente.Domain.Entities;

    public interface IMovimentoRepository
    {
        Task AdicionarMovimentoAsync(Movimento movimento);
        Task<IEnumerable<Movimento>> ObterMovimentosPorContaAsync(string contaCorrente);
        Task<decimal> ObterSaldoAsync(string contaCorrente);
        Task<decimal> ObterSaldoAteDataAsync(string contaCorrente, DateTime data);
        Task<IEnumerable<Movimento>> ObterMovimentosPorPeriodoAsync(string contaCorrente, DateTime dataInicio, DateTime dataFim, string? tipoMovimento, int offset, int limit);
        Task<int> ContarMovimentosPorPeriodoAsync(string contaCorrente, DateTime dataInicio, DateTime dataFim, string? tipoMovimento);
    }
}
