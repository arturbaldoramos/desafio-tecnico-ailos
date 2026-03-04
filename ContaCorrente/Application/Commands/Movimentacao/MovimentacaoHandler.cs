using ContaCorrente.Application.DTOs;
using ContaCorrente.Domain.Entities;
using ContaCorrente.Domain.Interfaces;
using MediatR;

namespace ContaCorrente.Application.Commands.Movimentacao
{
    public class MovimentacaoHandler : IRequestHandler<MovimentacaoCommand, IResult>
    {
        private readonly IContaRepository _contaRepository;
        private readonly IMovimentoRepository _movimentoRepository;
        private readonly IIdempotenciaRepository _idempotenciaRepository;
        private readonly ISaldoCacheService _saldoCache;

        public MovimentacaoHandler(
            IContaRepository contaRepository,
            IMovimentoRepository movimentoRepository,
            IIdempotenciaRepository idempotenciaRepository,
            ISaldoCacheService saldoCache)
        {
            _contaRepository = contaRepository;
            _movimentoRepository = movimentoRepository;
            _idempotenciaRepository = idempotenciaRepository;
            _saldoCache = saldoCache;
        }

        public async Task<IResult> Handle(MovimentacaoCommand request, CancellationToken cancellationToken)
        {
            // Verificar idempotência
            var idempotencia = await _idempotenciaRepository.ObterPorChaveAsync(request.IdRequisicao);
            if (idempotencia != null)
            {
                return Results.Ok(new { mensagem = "Requisição já processada", idRequisicao = request.IdRequisicao });
            }

            // Validar tipo de movimento
            if (request.Tipo != "C" && request.Tipo != "D")
            {
                return Results.BadRequest(new ErrorResponse("Tipo de movimento inválido. Use 'C' para Crédito ou 'D' para Débito", "INVALID_TYPE"));
            }

            // Validar valor
            if (request.Valor <= 0)
            {
                return Results.BadRequest(new ErrorResponse("O valor deve ser maior que zero", "INVALID_VALUE"));
            }

            // Buscar conta
            var conta = await _contaRepository.ObterPorNumeroAsync(request.NumeroConta);
            if (conta == null)
            {
                return Results.BadRequest(new ErrorResponse("Conta não encontrada", "INVALID_ACCOUNT"));
            }

            if (conta.Ativo == 0)
            {
                return Results.BadRequest(new ErrorResponse("Conta inativa", "INACTIVE_ACCOUNT"));
            }

            // Criar movimento
            var movimento = new Movimento
            {
                IdContaCorrente = conta.IdContaCorrente.ToString(),
                DataMovimento = DateTime.UtcNow,
                TipoMovimento = request.Tipo,
                Valor = request.Valor
            };

            await _movimentoRepository.AdicionarMovimentoAsync(movimento);

            // Invalidar cache de saldo após movimentação
            _saldoCache.InvalidarSaldo(request.NumeroConta);

            // Salvar idempotência
            await _idempotenciaRepository.SalvarAsync(new Idempotencia
            {
                ChaveIdempotencia = request.IdRequisicao,
                Requisicao = $"Movimentacao:{request.NumeroConta}:{request.Tipo}:{request.Valor}",
                Resultado = "OK"
            });

            return Results.NoContent();
        }
    }
}
