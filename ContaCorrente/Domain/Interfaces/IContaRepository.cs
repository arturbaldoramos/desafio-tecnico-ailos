namespace ContaCorrente.Domain.Interfaces
{
    using ContaCorrente.Domain.Entities;

    public interface IContaRepository
    {
        Task<ContaCorrente> ObterPorCpfAsync(string cpf);
        Task<ContaCorrente> ObterPorNomeAsync(string nome);
        Task<ContaCorrente> ObterPorNumeroAsync(string numero);
        Task<int> AdicionarContaAsync(ContaCorrente conta);
        Task InativarContaAsync(string numero);
    }
}
