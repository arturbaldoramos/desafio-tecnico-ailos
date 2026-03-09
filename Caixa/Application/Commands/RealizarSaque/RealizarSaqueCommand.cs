using MediatR;

namespace Caixa.Application.Commands.RealizarSaque
{
    public record RealizarSaqueCommand(
        string IdRequisicao,
        string NumeroConta,
        decimal Valor
    ) : IRequest<IResult>;
}
