using Caixa.Application.DTOs;
using Caixa.Domain.Entities;
using Caixa.Domain.Interfaces;
using Caixa.Infrastructure.Messaging;
using Caixa.Infrastructure.Messaging.Messages;
using KafkaFlow.Producers;
using MediatR;

namespace Caixa.Application.Commands.RealizarDeposito
{
    public class RealizarDepositoHandler : IRequestHandler<RealizarDepositoCommand, IResult>
    {
        private readonly IDepositoRepository _depositoRepository;
        private readonly IIdempotenciaRepository _idempotenciaRepository;
        private readonly IProducerAccessor _producerAccessor;

        public RealizarDepositoHandler(
            IDepositoRepository depositoRepository,
            IIdempotenciaRepository idempotenciaRepository,
            IProducerAccessor producerAccessor)
        {
            _depositoRepository = depositoRepository;
            _idempotenciaRepository = idempotenciaRepository;
            _producerAccessor = producerAccessor;
        }

        public async Task<IResult> Handle(RealizarDepositoCommand request, CancellationToken cancellationToken)
        {
            var chaveIdempotencia = $"DEPOSITO-{request.IdRequisicao}";

            var idempotencia = await _idempotenciaRepository.ObterPorChaveAsync(chaveIdempotencia);
            if (idempotencia != null)
            {
                var existente = await _depositoRepository.ObterPorIdRequisicaoAsync(request.IdRequisicao);
                if (existente != null)
                    return Results.Ok(new DepositoResponse(existente.IdRequisicao, existente.NumeroConta, existente.Valor, existente.Status, existente.DataDeposito));

                return Results.Ok(new { mensagem = "Requisição já processada", idRequisicao = request.IdRequisicao });
            }

            if (request.Valor <= 0)
                return Results.BadRequest(new ErrorResponse("O valor deve ser maior que zero", "INVALID_VALUE"));

            if (request.Valor > 50000)
                return Results.BadRequest(new ErrorResponse("O valor máximo por depósito é R$ 50.000,00", "MAX_VALUE_EXCEEDED"));

            var deposito = new Domain.Entities.Deposito
            {
                IdRequisicao = request.IdRequisicao,
                NumeroConta = request.NumeroConta,
                Valor = request.Valor,
                DataDeposito = DateTime.UtcNow,
                Status = "PENDENTE"
            };

            await _depositoRepository.AdicionarAsync(deposito);

            await _idempotenciaRepository.SalvarAsync(new Idempotencia
            {
                ChaveIdempotencia = chaveIdempotencia,
                Requisicao = $"Deposito:{request.NumeroConta}:{request.Valor}",
                Resultado = "PENDENTE"
            });

            var mensagem = new DepositoSolicitadoMessage
            {
                IdRequisicao = request.IdRequisicao,
                NumeroConta = request.NumeroConta,
                Valor = request.Valor,
                DataSolicitacao = deposito.DataDeposito
            };

            var producer = _producerAccessor.GetProducer("deposito-solicitado-producer");
            await producer.ProduceAsync(KafkaTopics.DepositoSolicitado, request.IdRequisicao, mensagem);

            return Results.Accepted($"/api/caixa/deposito/{request.IdRequisicao}",
                new DepositoResponse(deposito.IdRequisicao, deposito.NumeroConta, deposito.Valor, deposito.Status, deposito.DataDeposito));
        }
    }
}
