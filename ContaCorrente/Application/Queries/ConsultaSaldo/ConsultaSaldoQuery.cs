using MediatR;

namespace ContaCorrente.Application.Queries.ConsultaSaldo
{
    public record ObterSaldoQuery(string NumeroConta) : IRequest<IResult>;
}
