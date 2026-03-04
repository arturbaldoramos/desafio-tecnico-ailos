using KafkaFlow;
using KafkaFlow.Producers;
using MediatR;
using Transferencia.Application.DTOs;
using Transferencia.Domain.Entities;
using Transferencia.Domain.Interfaces;
using Transferencia.Infrastructure.Messaging;
using Transferencia.Infrastructure.Messaging.Messages;

namespace Transferencia.Application.Commands.RealizarTransferencia
{
    public class RealizarTransferenciaHandler : IRequestHandler<RealizarTransferenciaCommand, IResult>
    {
        private readonly ITransferenciaRepository _transferenciaRepository;
        private readonly IIdempotenciaRepository _idempotenciaRepository;
        private readonly IProducerAccessor _producerAccessor;
        private readonly ILogger<RealizarTransferenciaHandler> _logger;

        public RealizarTransferenciaHandler(
            ITransferenciaRepository transferenciaRepository,
            IIdempotenciaRepository idempotenciaRepository,
            IProducerAccessor producerAccessor,
            ILogger<RealizarTransferenciaHandler> logger)
        {
            _transferenciaRepository = transferenciaRepository;
            _idempotenciaRepository = idempotenciaRepository;
            _producerAccessor = producerAccessor;
            _logger = logger;
        }

        public async Task<IResult> Handle(RealizarTransferenciaCommand request, CancellationToken cancellationToken)
        {
            // Verificar idempotência
            var idempotencia = await _idempotenciaRepository.ObterPorChaveAsync(request.IdRequisicao);
            if (idempotencia != null)
            {
                // Buscar a transferência existente para retornar o status atual
                var transferenciaExistente = await _transferenciaRepository.ObterPorIdRequisicaoAsync(request.IdRequisicao);
                if (transferenciaExistente != null)
                {
                    return Results.Ok(new TransferenciaResponse(
                        transferenciaExistente.IdTransferencia,
                        transferenciaExistente.NumeroContaOrigem,
                        transferenciaExistente.NumeroContaDestino,
                        transferenciaExistente.Valor,
                        transferenciaExistente.DataMovimento,
                        transferenciaExistente.Status
                    ));
                }
                return Results.Ok(new { mensagem = "Requisição já processada", idRequisicao = request.IdRequisicao });
            }

            // Validar valor
            if (request.Valor <= 0)
            {
                return Results.BadRequest(new ErrorResponse("O valor deve ser maior que zero", "INVALID_VALUE"));
            }

            // Validar que não está transferindo para si mesmo
            if (request.NumeroContaOrigem == request.ContaDestino)
            {
                return Results.BadRequest(new ErrorResponse("Não é possível transferir para a mesma conta", "SAME_ACCOUNT"));
            }

            // Criar registro da transferência com status PENDENTE
            var transferencia = new Domain.Entities.Transferencia
            {
                IdRequisicao = request.IdRequisicao,
                NumeroContaOrigem = request.NumeroContaOrigem,
                NumeroContaDestino = request.ContaDestino,
                DataMovimento = DateTime.UtcNow,
                Valor = request.Valor,
                Status = TransferenciaStatus.Pendente
            };

            await _transferenciaRepository.AdicionarAsync(transferencia);

            // Salvar idempotência
            await _idempotenciaRepository.SalvarAsync(new Idempotencia
            {
                ChaveIdempotencia = request.IdRequisicao,
                Requisicao = $"Transferencia:{request.NumeroContaOrigem}:{request.ContaDestino}:{request.Valor}",
                Resultado = "PENDENTE"
            });

            // Publicar mensagem no Kafka
            var mensagem = new TransferenciaSolicitadaMessage
            {
                IdRequisicao = request.IdRequisicao,
                NumeroContaOrigem = request.NumeroContaOrigem,
                NumeroContaDestino = request.ContaDestino,
                Valor = request.Valor,
                DataSolicitacao = transferencia.DataMovimento
            };

            var producer = _producerAccessor.GetProducer("transferencia-solicitada-producer");
            await producer.ProduceAsync(KafkaTopics.TransferenciasSolicitadas, request.IdRequisicao, mensagem);

            _logger.LogInformation("Transferência {IdRequisicao} publicada no Kafka", request.IdRequisicao);

            return Results.Accepted(
                $"/api/transferencia/{request.IdRequisicao}",
                new TransferenciaResponse(
                    transferencia.IdTransferencia,
                    transferencia.NumeroContaOrigem,
                    transferencia.NumeroContaDestino,
                    transferencia.Valor,
                    transferencia.DataMovimento,
                    transferencia.Status
                )
            );
        }
    }
}
