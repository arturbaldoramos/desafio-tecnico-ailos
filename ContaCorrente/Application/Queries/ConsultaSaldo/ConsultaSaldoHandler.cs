using ContaCorrente.Application.DTOs;
using ContaCorrente.Domain.Interfaces;
using MediatR;

namespace ContaCorrente.Application.Queries.ConsultaSaldo
{
    public class ObterSaldoHandler : IRequestHandler<ObterSaldoQuery, IResult>
    {
        private readonly IContaRepository _contaRepository;
        private readonly IMovimentoRepository _movimentoRepository;
        private readonly ISaldoCacheService _saldoCache;

        public ObterSaldoHandler(
            IContaRepository contaRepository,
            IMovimentoRepository movimentoRepository,
            ISaldoCacheService saldoCache)
        {
            _contaRepository = contaRepository;
            _movimentoRepository = movimentoRepository;
            _saldoCache = saldoCache;
        }

        public async Task<IResult> Handle(ObterSaldoQuery request, CancellationToken cancellationToken)
        {
            var conta = await _contaRepository.ObterPorNumeroAsync(request.NumeroConta);

            if (conta == null)
                return Results.BadRequest(new ErrorResponse("Conta não encontrada", "INVALID_ACCOUNT"));

            if (conta.Ativo == 0)
                return Results.BadRequest(new ErrorResponse("Conta inativa", "INACTIVE_ACCOUNT"));

            // Tentar obter do cache primeiro
            var saldoCache = await _saldoCache.ObterSaldoAsync(request.NumeroConta);
            if (saldoCache.HasValue)
            {
                return Results.Ok(new SaldoResponse(conta.Numero, saldoCache.Value));
            }

            // Se não está em cache, calcular do banco
            var movimentos = await _movimentoRepository.ObterMovimentosPorContaAsync(conta.IdContaCorrente);

            var creditos = movimentos.Where(m => m.TipoMovimento == "C").Sum(m => m.Valor);
            var debitos = movimentos.Where(m => m.TipoMovimento == "D").Sum(m => m.Valor);
            var saldoTotal = creditos - debitos;

            // Armazenar no cache para próximas consultas
            await _saldoCache.DefinirSaldoAsync(request.NumeroConta, saldoTotal);

            return Results.Ok(new SaldoResponse(conta.Numero, saldoTotal));
        }
    }
}
