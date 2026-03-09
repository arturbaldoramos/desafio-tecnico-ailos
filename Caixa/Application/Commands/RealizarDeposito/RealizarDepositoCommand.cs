using MediatR;

namespace Caixa.Application.Commands.RealizarDeposito
{
    public record RealizarDepositoCommand(
        string IdRequisicao,
        string NumeroConta,
        decimal Valor
    ) : IRequest<IResult>;
}
