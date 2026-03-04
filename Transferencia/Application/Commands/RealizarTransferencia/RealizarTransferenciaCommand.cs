using MediatR;

namespace Transferencia.Application.Commands.RealizarTransferencia
{
    public record RealizarTransferenciaCommand(
        string IdRequisicao,
        string NumeroContaOrigem,
        string ContaDestino,
        decimal Valor
    ) : IRequest<IResult>;
}
