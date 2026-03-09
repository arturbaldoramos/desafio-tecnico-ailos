using Caixa.Domain.Entities;

namespace Caixa.Domain.Interfaces
{
    public interface IDepositoRepository
    {
        Task AdicionarAsync(Entities.Deposito deposito);
        Task<Entities.Deposito?> ObterPorIdRequisicaoAsync(string idRequisicao);
        Task AtualizarStatusAsync(string idRequisicao, string status, string? mensagemErro = null);
    }
}
