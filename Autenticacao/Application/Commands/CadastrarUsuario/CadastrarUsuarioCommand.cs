using MediatR;

namespace Autenticacao.Application.Commands.CadastrarUsuario
{
    public record CadastrarUsuarioCommand(string Nome, string Cpf, string Senha) : IRequest<IResult>;
}
