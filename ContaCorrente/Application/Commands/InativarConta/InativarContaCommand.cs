using MediatR;

namespace ContaCorrente.Application.Commands.InativarConta
{
    public record InativarContaCommand(string NumeroConta, string Senha) : IRequest<IResult>;
}
