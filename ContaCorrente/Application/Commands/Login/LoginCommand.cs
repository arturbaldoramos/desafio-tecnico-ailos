using MediatR;

namespace ContaCorrente.Application.Commands.Login
{
    public record LoginCommand(string CpfOuNumero, string Senha) : IRequest<IResult>;
}
