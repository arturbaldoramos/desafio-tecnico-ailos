using Caixa.Domain.Entities;

namespace Caixa.Domain.Interfaces
{
    public interface ISaqueRepository
    {
        Task AdicionarAsync(Saque saque);
        Task<Saque?> ObterPorIdRequisicaoAsync(string idRequisicao);
        Task AtualizarStatusAsync(string idRequisicao, string status, string? mensagemErro = null);
    }
}
