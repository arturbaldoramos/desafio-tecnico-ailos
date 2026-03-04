namespace ContaCorrente.Domain.Interfaces
{
    using ContaCorrente.Domain.Entities;

    public interface IIdempotenciaRepository
    {
        Task<Idempotencia?> ObterPorChaveAsync(string chave);
        Task SalvarAsync(Idempotencia idempotencia);
    }
}
