using KafkaFlow;
using KafkaFlow.Producers;
using Transferencia.Domain.Entities;
using Transferencia.Domain.Interfaces;
using Transferencia.Infrastructure.Messaging.Messages;

namespace Transferencia.Infrastructure.Messaging.Consumers
{
    public class TransferenciaResultadoConsumer : IMessageHandler<TransferenciaResultadoMessage>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TransferenciaResultadoConsumer> _logger;

        public TransferenciaResultadoConsumer(IServiceProvider serviceProvider, ILogger<TransferenciaResultadoConsumer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task Handle(IMessageContext context, TransferenciaResultadoMessage message)
        {
            _logger.LogInformation("Recebido resultado da transferência {IdRequisicao}: Sucesso={Sucesso}",
                message.IdRequisicao, message.Sucesso);

            using var scope = _serviceProvider.CreateScope();
            var transferenciaRepository = scope.ServiceProvider.GetRequiredService<ITransferenciaRepository>();
            var producerAccessor = scope.ServiceProvider.GetRequiredService<IProducerAccessor>();

            var status = message.Sucesso ? TransferenciaStatus.Sucesso : TransferenciaStatus.Erro;

            await transferenciaRepository.AtualizarStatusAsync(
                message.IdRequisicao,
                status,
                message.MensagemErro
            );

            _logger.LogInformation("Status da transferência {IdRequisicao} atualizado para {Status}",
                message.IdRequisicao, status);

            // Se a transferência foi bem-sucedida, publicar evento para cobrança de tarifa
            if (message.Sucesso)
            {
                var transferencia = await transferenciaRepository.ObterPorIdRequisicaoAsync(message.IdRequisicao);
                if (transferencia != null)
                {
                    var mensagemRealizada = new TransferenciaRealizadaMessage
                    {
                        IdRequisicao = message.IdRequisicao,
                        NumeroContaOrigem = transferencia.NumeroContaOrigem,
                        ValorTransferencia = transferencia.Valor,
                        DataTransferencia = transferencia.DataMovimento
                    };

                    var producer = producerAccessor.GetProducer("transferencia-realizada-producer");
                    await producer.ProduceAsync(KafkaTopics.TransferenciasRealizadas, message.IdRequisicao, mensagemRealizada);

                    _logger.LogInformation("Evento transferencia-realizada publicado para {IdRequisicao}", message.IdRequisicao);
                }
            }
        }
    }
}
