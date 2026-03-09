using Caixa.Domain.Entities;

namespace Caixa.Domain.Interfaces
{
    public interface IIdempotenciaRepository
    {
        Task<Idempotencia?> ObterPorChaveAsync(string chave);
        Task SalvarAsync(Idempotencia idempotencia);
    }
}
