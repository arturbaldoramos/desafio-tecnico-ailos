using KafkaFlow;
using KafkaFlow.Producers;
using Tarifa.Domain.Entities;
using Tarifa.Domain.Interfaces;
using Tarifa.Infrastructure.Messaging.Messages;

namespace Tarifa.Infrastructure.Messaging.Consumers
{
    public class TransferenciaRealizadaConsumer : IMessageHandler<TransferenciaRealizadaMessage>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TransferenciaRealizadaConsumer> _logger;
        private readonly IConfiguration _configuration;

        public TransferenciaRealizadaConsumer(
            IServiceProvider serviceProvider,
            ILogger<TransferenciaRealizadaConsumer> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task Handle(IMessageContext context, TransferenciaRealizadaMessage message)
        {
            _logger.LogInformation("Processando tarifacao para transferencia {IdRequisicao}, Conta: {NumeroContaOrigem}",
                message.IdRequisicao, message.NumeroContaOrigem);

            using var scope = _serviceProvider.CreateScope();
            var tarifacaoRepository = scope.ServiceProvider.GetRequiredService<ITarifacaoRepository>();
            var idempotenciaRepository = scope.ServiceProvider.GetRequiredService<IIdempotenciaRepository>();
            var producerAccessor = scope.ServiceProvider.GetRequiredService<IProducerAccessor>();

            // Chave de idempotencia para evitar tarifacao duplicada
            var chaveIdempotencia = $"TARIFA-{message.IdRequisicao}";

            // Verificar se ja processamos esta tarifacao
            var idempotencia = await idempotenciaRepository.ObterPorChaveAsync(chaveIdempotencia);
            if (idempotencia != null)
            {
                _logger.LogWarning("Tarifacao {IdRequisicao} ja foi processada", message.IdRequisicao);
                return;
            }

            try
            {
                // Ler valor da tarifa do appsettings
                var valorTarifa = _configuration.GetValue<decimal>("Tarifa:ValorTransferencia");

                // Criar registro de tarifacao
                var tarifacao = new Tarifacao
                {
                    NumeroContaCorrente = message.NumeroContaOrigem,
                    IdRequisicaoTransferencia = message.IdRequisicao,
                    Valor = valorTarifa,
                    DataTarifacao = DateTime.UtcNow
                };

                await tarifacaoRepository.AdicionarAsync(tarifacao);

                // Salvar idempotencia
                await idempotenciaRepository.SalvarAsync(new Idempotencia
                {
                    ChaveIdempotencia = chaveIdempotencia,
                    Requisicao = $"Tarifacao:{message.NumeroContaOrigem}:{valorTarifa}",
                    Resultado = "OK"
                });

                // Publicar mensagem para debito da tarifa na conta corrente
                var mensagemTarifacao = new TarifacaoRealizadaMessage
                {
                    NumeroContaCorrente = message.NumeroContaOrigem,
                    ValorTarifa = valorTarifa,
                    IdRequisicaoOrigem = message.IdRequisicao,
                    DataTarifacao = tarifacao.DataTarifacao
                };

                var producer = producerAccessor.GetProducer("tarifacao-realizada-producer");
                await producer.ProduceAsync(KafkaTopics.TarifacoesRealizadas, message.IdRequisicao, mensagemTarifacao);

                _logger.LogInformation("Tarifacao {IdRequisicao} processada com sucesso. Valor: {ValorTarifa}",
                    message.IdRequisicao, valorTarifa);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar tarifacao {IdRequisicao}", message.IdRequisicao);
                throw;
            }
        }
    }
}
