using MediatR;

namespace Autenticacao.Application.Commands.Login
{
    public record LoginCommand(string CpfOuNumero, string Senha) : IRequest<IResult>;
}
