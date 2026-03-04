using MediatR;

namespace ContaCorrente.Application.Commands.CadastrarConta
{
    public record CadastrarContaCommand(string Nome, string Cpf, string Senha) : IRequest<IResult>;
}
