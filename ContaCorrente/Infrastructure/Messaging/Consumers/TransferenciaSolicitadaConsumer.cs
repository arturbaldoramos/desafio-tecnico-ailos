using ContaCorrente.Domain.Entities;
using ContaCorrente.Domain.Interfaces;
using ContaCorrente.Infrastructure.Messaging.Messages;
using KafkaFlow;
using KafkaFlow.Producers;

namespace ContaCorrente.Infrastructure.Messaging.Consumers
{
    public class TransferenciaSolicitadaConsumer : IMessageHandler<TransferenciaSolicitadaMessage>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TransferenciaSolicitadaConsumer> _logger;
        private readonly ISaldoCacheService _saldoCache;

        public TransferenciaSolicitadaConsumer(
            IServiceProvider serviceProvider,
            ILogger<TransferenciaSolicitadaConsumer> logger,
            ISaldoCacheService saldoCache)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _saldoCache = saldoCache;
        }

        public async Task Handle(IMessageContext context, TransferenciaSolicitadaMessage message)
        {
            _logger.LogInformation("Processando transferência {IdRequisicao}: {ContaOrigem} -> {ContaDestino}, Valor: {Valor}",
                message.IdRequisicao, message.NumeroContaOrigem, message.NumeroContaDestino, message.Valor);

            using var scope = _serviceProvider.CreateScope();
            var contaRepository = scope.ServiceProvider.GetRequiredService<IContaRepository>();
            var movimentoRepository = scope.ServiceProvider.GetRequiredService<IMovimentoRepository>();
            var idempotenciaRepository = scope.ServiceProvider.GetRequiredService<IIdempotenciaRepository>();
            var producerAccessor = scope.ServiceProvider.GetRequiredService<IProducerAccessor>();

            var resultado = new TransferenciaResultadoMessage
            {
                IdRequisicao = message.IdRequisicao,
                DataProcessamento = DateTime.UtcNow
            };

            try
            {
                // Buscar conta origem
                var contaOrigem = await contaRepository.ObterPorNumeroAsync(message.NumeroContaOrigem);
                if (contaOrigem == null || contaOrigem.Ativo == 0)
                {
                    resultado.Sucesso = false;
                    resultado.MensagemErro = "Conta origem não encontrada ou inativa";
                    await PublicarResultado(producerAccessor, resultado);
                    return;
                }

                // Buscar conta destino
                var contaDestino = await contaRepository.ObterPorNumeroAsync(message.NumeroContaDestino);
                if (contaDestino == null || contaDestino.Ativo == 0)
                {
                    resultado.Sucesso = false;
                    resultado.MensagemErro = "Conta destino não encontrada ou inativa";
                    await PublicarResultado(producerAccessor, resultado);
                    return;
                }

                // Chaves de idempotência para as movimentações da transferência
                var chaveIdempotenciaDebito = $"TRANSF-{message.IdRequisicao}-D";
                var chaveIdempotenciaCredito = $"TRANSF-{message.IdRequisicao}-C";

                // Verificar se já processamos esta transferência (evita reprocessamento em caso de retry do Kafka)
                var idempotenciaDebito = await idempotenciaRepository.ObterPorChaveAsync(chaveIdempotenciaDebito);
                if (idempotenciaDebito != null)
                {
                    _logger.LogWarning("Transferência {IdRequisicao} já foi processada no ContaCorrente", message.IdRequisicao);
                    resultado.Sucesso = true;
                    resultado.IdContaCorrenteDestino = contaDestino.IdContaCorrente;
                    await PublicarResultado(producerAccessor, resultado);
                    return;
                }

                // Criar movimento de DÉBITO na conta origem
                var movimentoDebito = new Movimento
                {
                    IdContaCorrente = contaOrigem.IdContaCorrente.ToString(),
                    DataMovimento = DateTime.UtcNow,
                    TipoMovimento = "D",
                    Valor = message.Valor
                };
                await movimentoRepository.AdicionarMovimentoAsync(movimentoDebito);

                // Salvar idempotência do débito
                await idempotenciaRepository.SalvarAsync(new Idempotencia
                {
                    ChaveIdempotencia = chaveIdempotenciaDebito,
                    Requisicao = $"Debito:{message.NumeroContaOrigem}:{message.Valor}",
                    Resultado = "OK"
                });

                // Criar movimento de CRÉDITO na conta destino
                var movimentoCredito = new Movimento
                {
                    IdContaCorrente = contaDestino.IdContaCorrente.ToString(),
                    DataMovimento = DateTime.UtcNow,
                    TipoMovimento = "C",
                    Valor = message.Valor
                };
                await movimentoRepository.AdicionarMovimentoAsync(movimentoCredito);

                // Salvar idempotência do crédito
                await idempotenciaRepository.SalvarAsync(new Idempotencia
                {
                    ChaveIdempotencia = chaveIdempotenciaCredito,
                    Requisicao = $"Credito:{message.NumeroContaDestino}:{message.Valor}",
                    Resultado = "OK"
                });

                // Invalidar cache de saldo das contas envolvidas
                _saldoCache.InvalidarSaldo(message.NumeroContaOrigem);
                _saldoCache.InvalidarSaldo(message.NumeroContaDestino);

                resultado.Sucesso = true;
                resultado.IdContaCorrenteDestino = contaDestino.IdContaCorrente;

                _logger.LogInformation("Transferência {IdRequisicao} processada com sucesso", message.IdRequisicao);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar transferência {IdRequisicao}", message.IdRequisicao);
                resultado.Sucesso = false;
                resultado.MensagemErro = ex.Message;
            }

            await PublicarResultado(producerAccessor, resultado);
        }

        private async Task PublicarResultado(IProducerAccessor producerAccessor, TransferenciaResultadoMessage resultado)
        {
            var producer = producerAccessor.GetProducer("transferencia-resultado-producer");
            await producer.ProduceAsync(KafkaTopics.TransferenciasResultado, resultado.IdRequisicao, resultado);
        }
    }
}
