using MediatR;

namespace ContaCorrente.Application.Commands.Movimentacao
{
    public record MovimentacaoCommand(
        string IdRequisicao,
        string NumeroConta,
        string Tipo, // "C" para Crédito, "D" para Débito
        decimal Valor
    ) : IRequest<IResult>;
}
