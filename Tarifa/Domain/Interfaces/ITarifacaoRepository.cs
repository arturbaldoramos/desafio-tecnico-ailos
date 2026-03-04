using Tarifa.Domain.Entities;

namespace Tarifa.Domain.Interfaces
{
    public interface ITarifacaoRepository
    {
        Task AdicionarAsync(Tarifacao tarifacao);
        Task<Tarifacao?> ObterPorIdRequisicaoAsync(string idRequisicaoTransferencia);
    }
}
