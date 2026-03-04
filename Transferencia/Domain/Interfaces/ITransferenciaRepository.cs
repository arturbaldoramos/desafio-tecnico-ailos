namespace Transferencia.Domain.Interfaces
{
    public interface ITransferenciaRepository
    {
        Task AdicionarAsync(Entities.Transferencia transferencia);
        Task<Entities.Transferencia?> ObterPorIdAsync(int id);
        Task<Entities.Transferencia?> ObterPorIdRequisicaoAsync(string idRequisicao);
        Task AtualizarStatusAsync(string idRequisicao, string status, string? mensagemErro = null);
    }
}
