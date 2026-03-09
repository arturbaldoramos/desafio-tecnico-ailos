using Caixa.Application.DTOs;
using Caixa.Domain.Entities;
using Caixa.Domain.Interfaces;
using Caixa.Infrastructure.Messaging;
using Caixa.Infrastructure.Messaging.Messages;
using KafkaFlow.Producers;
using MediatR;

namespace Caixa.Application.Commands.RealizarSaque
{
    public class RealizarSaqueHandler : IRequestHandler<RealizarSaqueCommand, IResult>
    {
        private readonly ISaqueRepository _saqueRepository;
        private readonly IIdempotenciaRepository _idempotenciaRepository;
        private readonly IProducerAccessor _producerAccessor;

        public RealizarSaqueHandler(
            ISaqueRepository saqueRepository,
            IIdempotenciaRepository idempotenciaRepository,
            IProducerAccessor producerAccessor)
        {
            _saqueRepository = saqueRepository;
            _idempotenciaRepository = idempotenciaRepository;
            _producerAccessor = producerAccessor;
        }

        public async Task<IResult> Handle(RealizarSaqueCommand request, CancellationToken cancellationToken)
        {
            var chaveIdempotencia = $"SAQUE-{request.IdRequisicao}";

            var idempotencia = await _idempotenciaRepository.ObterPorChaveAsync(chaveIdempotencia);
            if (idempotencia != null)
            {
                var existente = await _saqueRepository.ObterPorIdRequisicaoAsync(request.IdRequisicao);
                if (existente != null)
                    return Results.Ok(new SaqueResponse(existente.IdRequisicao, existente.NumeroConta, existente.Valor, existente.Status, existente.DataSaque));

                return Results.Ok(new { mensagem = "Requisição já processada", idRequisicao = request.IdRequisicao });
            }

            if (request.Valor <= 0)
                return Results.BadRequest(new ErrorResponse("O valor deve ser maior que zero", "INVALID_VALUE"));

            var saque = new Saque
            {
                IdRequisicao = request.IdRequisicao,
                NumeroConta = request.NumeroConta,
                Valor = request.Valor,
                DataSaque = DateTime.UtcNow,
                Status = "PENDENTE"
            };

            await _saqueRepository.AdicionarAsync(saque);

            await _idempotenciaRepository.SalvarAsync(new Idempotencia
            {
                ChaveIdempotencia = chaveIdempotencia,
                Requisicao = $"Saque:{request.NumeroConta}:{request.Valor}",
                Resultado = "PENDENTE"
            });

            var mensagem = new SaqueSolicitadoMessage
            {
                IdRequisicao = request.IdRequisicao,
                NumeroConta = request.NumeroConta,
                Valor = request.Valor,
                DataSolicitacao = saque.DataSaque
            };

            var producer = _producerAccessor.GetProducer("saque-solicitado-producer");
            await producer.ProduceAsync(KafkaTopics.SaqueSolicitado, request.IdRequisicao, mensagem);

            return Results.Accepted($"/api/caixa/saque/{request.IdRequisicao}",
                new SaqueResponse(saque.IdRequisicao, saque.NumeroConta, saque.Valor, saque.Status, saque.DataSaque));
        }
    }
}
