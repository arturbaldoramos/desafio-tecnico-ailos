using Tarifa.Domain.Entities;

namespace Tarifa.Domain.Interfaces
{
    public interface IIdempotenciaRepository
    {
        Task<Idempotencia?> ObterPorChaveAsync(string chave);
        Task SalvarAsync(Idempotencia idempotencia);
    }
}
