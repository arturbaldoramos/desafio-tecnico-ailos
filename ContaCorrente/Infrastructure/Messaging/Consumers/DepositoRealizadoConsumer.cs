using ContaCorrente.Domain.Entities;
using ContaCorrente.Domain.Interfaces;
using ContaCorrente.Infrastructure.Messaging.Messages;
using KafkaFlow;
using KafkaFlow.Producers;

namespace ContaCorrente.Infrastructure.Messaging.Consumers
{
    public class DepositoSolicitadoConsumer : IMessageHandler<DepositoSolicitadoMessage>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DepositoSolicitadoConsumer> _logger;
        private readonly ISaldoCacheService _saldoCache;

        public DepositoSolicitadoConsumer(
            IServiceProvider serviceProvider,
            ILogger<DepositoSolicitadoConsumer> logger,
            ISaldoCacheService saldoCache)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _saldoCache = saldoCache;
        }

        public async Task Handle(IMessageContext context, DepositoSolicitadoMessage message)
        {
            _logger.LogInformation("Processando depósito solicitado {IdRequisicao} para conta {NumeroConta}, Valor: {Valor}",
                message.IdRequisicao, message.NumeroConta, message.Valor);

            using var scope = _serviceProvider.CreateScope();
            var contaRepository = scope.ServiceProvider.GetRequiredService<IContaRepository>();
            var movimentoRepository = scope.ServiceProvider.GetRequiredService<IMovimentoRepository>();
            var idempotenciaRepository = scope.ServiceProvider.GetRequiredService<IIdempotenciaRepository>();
            var producerAccessor = scope.ServiceProvider.GetRequiredService<IProducerAccessor>();

            var resultado = new DepositoResultadoMessage
            {
                IdRequisicao = message.IdRequisicao,
                DataProcessamento = DateTime.UtcNow
            };

            try
            {
                var chaveIdempotencia = $"DEPOSITO-{message.IdRequisicao}";

                var idempotencia = await idempotenciaRepository.ObterPorChaveAsync(chaveIdempotencia);
                if (idempotencia != null)
                {
                    _logger.LogWarning("Depósito {IdRequisicao} já foi processado no ContaCorrente", message.IdRequisicao);
                    resultado.Sucesso = true;
                    await PublicarResultado(producerAccessor, resultado);
                    return;
                }

                var conta = await contaRepository.ObterPorNumeroAsync(message.NumeroConta);
                if (conta == null || conta.Ativo == 0)
                {
                    resultado.Sucesso = false;
                    resultado.MensagemErro = "Conta não encontrada ou inativa";
                    await PublicarResultado(producerAccessor, resultado);
                    return;
                }

                var movimento = new Movimento
                {
                    ContaCorrente = conta.Numero,
                    DataMovimento = message.DataSolicitacao,
                    TipoMovimento = "C",
                    Valor = message.Valor
                };

                await movimentoRepository.AdicionarMovimentoAsync(movimento);

                await idempotenciaRepository.SalvarAsync(new Idempotencia
                {
                    ChaveIdempotencia = chaveIdempotencia,
                    Requisicao = $"Deposito:{message.NumeroConta}:{message.Valor}",
                    Resultado = "OK"
                });

                _saldoCache.InvalidarSaldo(message.NumeroConta);

                resultado.Sucesso = true;

                _logger.LogInformation("Depósito {IdRequisicao} processado com sucesso. Conta: {NumeroConta}, Valor: {Valor}",
                    message.IdRequisicao, message.NumeroConta, message.Valor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar depósito {IdRequisicao}", message.IdRequisicao);
                resultado.Sucesso = false;
                resultado.MensagemErro = ex.Message;
            }

            await PublicarResultado(producerAccessor, resultado);
        }

        private async Task PublicarResultado(IProducerAccessor producerAccessor, DepositoResultadoMessage resultado)
        {
            var producer = producerAccessor.GetProducer("deposito-resultado-producer");
            await producer.ProduceAsync(KafkaTopics.DepositoResultado, resultado.IdRequisicao, resultado);
        }
    }
}
