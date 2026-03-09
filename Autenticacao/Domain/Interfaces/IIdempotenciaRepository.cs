namespace Autenticacao.Domain.Interfaces
{
    using Autenticacao.Domain.Entities;

    public interface IIdempotenciaRepository
    {
        Task<Idempotencia?> ObterPorChaveAsync(string chave);
        Task SalvarAsync(Idempotencia idempotencia);
    }
}
