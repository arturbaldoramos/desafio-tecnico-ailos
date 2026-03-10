using ContaCorrente.Application.DTOs;
using ContaCorrente.Domain.Interfaces;
using MediatR;

namespace ContaCorrente.Application.Queries.ConsultaExtrato
{
    public class ConsultaExtratoHandler : IRequestHandler<ConsultaExtratoQuery, IResult>
    {
        private readonly IContaRepository _contaRepository;
        private readonly IMovimentoRepository _movimentoRepository;

        public ConsultaExtratoHandler(
            IContaRepository contaRepository,
            IMovimentoRepository movimentoRepository)
        {
            _contaRepository = contaRepository;
            _movimentoRepository = movimentoRepository;
        }

        public async Task<IResult> Handle(ConsultaExtratoQuery request, CancellationToken cancellationToken)
        {
            var conta = await _contaRepository.ObterPorNumeroAsync(request.NumeroConta);

            if (conta == null)
                return Results.BadRequest(new ErrorResponse("Conta não encontrada", "INVALID_ACCOUNT"));

            if (conta.Ativo == 0)
                return Results.BadRequest(new ErrorResponse("Conta inativa", "INACTIVE_ACCOUNT"));

            var saldoAnterior = await _movimentoRepository.ObterSaldoAteDataAsync(conta.Numero, request.DataInicio);

            var totalRegistros = await _movimentoRepository.ContarMovimentosPorPeriodoAsync(
                conta.Numero, request.DataInicio, request.DataFim, request.TipoMovimento);

            var offset = (request.Pagina - 1) * request.TamanhoPagina;
            var movimentos = await _movimentoRepository.ObterMovimentosPorPeriodoAsync(
                conta.Numero, request.DataInicio, request.DataFim, request.TipoMovimento, offset, request.TamanhoPagina);

            var itens = movimentos.Select(m => new MovimentoExtratoItem(
                m.DataMovimento, m.TipoMovimento, m.Valor)).ToList();

            var saldoMovimentos = movimentos.Sum(m => m.TipoMovimento == "C" ? m.Valor : -m.Valor);
            var saldoFinal = saldoAnterior + saldoMovimentos;

            return Results.Ok(new ExtratoResponse(
                conta.Numero,
                saldoAnterior,
                saldoFinal,
                request.Pagina,
                request.TamanhoPagina,
                totalRegistros,
                itens));
        }
    }
}
