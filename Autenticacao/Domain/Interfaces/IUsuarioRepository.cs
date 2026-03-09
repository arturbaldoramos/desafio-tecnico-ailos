namespace Autenticacao.Domain.Interfaces
{
    using Autenticacao.Domain.Entities;

    public interface IUsuarioRepository
    {
        Task<Usuario?> ObterPorCpfAsync(string cpf);
        Task<Usuario?> ObterPorNumeroContaAsync(string numeroConta);
        Task<int> AdicionarAsync(Usuario usuario);
        Task InativarAsync(string numeroConta);
    }
}
