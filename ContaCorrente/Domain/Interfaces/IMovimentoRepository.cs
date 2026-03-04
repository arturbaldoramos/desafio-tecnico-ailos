namespace ContaCorrente.Domain.Interfaces
{
    using ContaCorrente.Domain.Entities;

    public interface IMovimentoRepository
    {
        Task AdicionarMovimentoAsync(Movimento movimento);
        Task<IEnumerable<Movimento>> ObterMovimentosPorContaAsync(int idContaCorrente);
    }
}
