using ContaCorrente.Domain.Entities;
using ContaCorrente.Domain.Interfaces;
using ContaCorrente.Infrastructure.Messaging.Messages;
using KafkaFlow;
using KafkaFlow.Producers;

namespace ContaCorrente.Infrastructure.Messaging.Consumers
{
    public class SaqueSolicitadoConsumer : IMessageHandler<SaqueSolicitadoMessage>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SaqueSolicitadoConsumer> _logger;
        private readonly ISaldoCacheService _saldoCache;

        public SaqueSolicitadoConsumer(
            IServiceProvider serviceProvider,
            ILogger<SaqueSolicitadoConsumer> logger,
            ISaldoCacheService saldoCache)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _saldoCache = saldoCache;
        }

        public async Task Handle(IMessageContext context, SaqueSolicitadoMessage message)
        {
            _logger.LogInformation("Processando saque solicitado {IdRequisicao} para conta {NumeroConta}, Valor: {Valor}",
                message.IdRequisicao, message.NumeroConta, message.Valor);

            using var scope = _serviceProvider.CreateScope();
            var contaRepository = scope.ServiceProvider.GetRequiredService<IContaRepository>();
            var movimentoRepository = scope.ServiceProvider.GetRequiredService<IMovimentoRepository>();
            var idempotenciaRepository = scope.ServiceProvider.GetRequiredService<IIdempotenciaRepository>();
            var producerAccessor = scope.ServiceProvider.GetRequiredService<IProducerAccessor>();

            var resultado = new SaqueResultadoMessage
            {
                IdRequisicao = message.IdRequisicao,
                DataProcessamento = DateTime.UtcNow
            };

            try
            {
                var chaveIdempotencia = $"SAQUE-{message.IdRequisicao}";

                var idempotencia = await idempotenciaRepository.ObterPorChaveAsync(chaveIdempotencia);
                if (idempotencia != null)
                {
                    _logger.LogWarning("Saque {IdRequisicao} já foi processado no ContaCorrente", message.IdRequisicao);
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

                // Validar saldo
                var saldo = await movimentoRepository.ObterSaldoAsync(conta.Numero);
                if (saldo < message.Valor)
                {
                    resultado.Sucesso = false;
                    resultado.MensagemErro = "Saldo insuficiente";
                    await PublicarResultado(producerAccessor, resultado);
                    return;
                }

                var movimento = new Movimento
                {
                    ContaCorrente = conta.Numero,
                    DataMovimento = message.DataSolicitacao,
                    TipoMovimento = "D",
                    Valor = message.Valor
                };

                await movimentoRepository.AdicionarMovimentoAsync(movimento);

                await idempotenciaRepository.SalvarAsync(new Idempotencia
                {
                    ChaveIdempotencia = chaveIdempotencia,
                    Requisicao = $"Saque:{message.NumeroConta}:{message.Valor}",
                    Resultado = "OK"
                });

                _saldoCache.InvalidarSaldo(message.NumeroConta);

                resultado.Sucesso = true;

                _logger.LogInformation("Saque {IdRequisicao} processado com sucesso. Conta: {NumeroConta}, Valor: {Valor}",
                    message.IdRequisicao, message.NumeroConta, message.Valor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar saque {IdRequisicao}", message.IdRequisicao);
                resultado.Sucesso = false;
                resultado.MensagemErro = ex.Message;
            }

            await PublicarResultado(producerAccessor, resultado);
        }

        private async Task PublicarResultado(IProducerAccessor producerAccessor, SaqueResultadoMessage resultado)
        {
            var producer = producerAccessor.GetProducer("saque-resultado-producer");
            await producer.ProduceAsync(KafkaTopics.SaqueResultado, resultado.IdRequisicao, resultado);
        }
    }
}
