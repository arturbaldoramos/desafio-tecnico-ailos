using Transferencia.Domain.Entities;

namespace Transferencia.Domain.Interfaces
{
    public interface IIdempotenciaRepository
    {
        Task<Idempotencia?> ObterPorChaveAsync(string chave);
        Task SalvarAsync(Idempotencia idempotencia);
    }
}
