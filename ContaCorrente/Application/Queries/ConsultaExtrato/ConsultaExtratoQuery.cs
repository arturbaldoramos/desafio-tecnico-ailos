using MediatR;

namespace ContaCorrente.Application.Queries.ConsultaExtrato
{
    public record ConsultaExtratoQuery(
        string NumeroConta,
        DateTime DataInicio,
        DateTime DataFim,
        string? TipoMovimento,
        int Pagina,
        int TamanhoPagina) : IRequest<IResult>;
}
