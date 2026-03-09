using MediatR;

namespace Autenticacao.Application.Commands.InativarUsuario
{
    public record InativarUsuarioCommand(string NumeroConta, string Senha) : IRequest<IResult>;
}
