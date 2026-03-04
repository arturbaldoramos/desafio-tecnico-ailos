using ContaCorrente.Domain.Entities;
using ContaCorrente.Domain.Interfaces;
using ContaCorrente.Infrastructure.Messaging.Messages;
using KafkaFlow;

namespace ContaCorrente.Infrastructure.Messaging.Consumers
{
    public class TarifacaoRealizadaConsumer : IMessageHandler<TarifacaoRealizadaMessage>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TarifacaoRealizadaConsumer> _logger;

        public TarifacaoRealizadaConsumer(IServiceProvider serviceProvider, ILogger<TarifacaoRealizadaConsumer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task Handle(IMessageContext context, TarifacaoRealizadaMessage message)
        {
            _logger.LogInformation("Processando debito de tarifa para conta {NumeroConta}, Valor: {ValorTarifa}",
                message.NumeroContaCorrente, message.ValorTarifa);

            using var scope = _serviceProvider.CreateScope();
            var contaRepository = scope.ServiceProvider.GetRequiredService<IContaRepository>();
            var movimentoRepository = scope.ServiceProvider.GetRequiredService<IMovimentoRepository>();
            var idempotenciaRepository = scope.ServiceProvider.GetRequiredService<IIdempotenciaRepository>();

            // Chave de idempotencia para evitar debito duplicado
            var chaveIdempotencia = $"TARIFA-DEBITO-{message.IdRequisicaoOrigem}";

            // Verificar se ja processamos este debito
            var idempotencia = await idempotenciaRepository.ObterPorChaveAsync(chaveIdempotencia);
            if (idempotencia != null)
            {
                _logger.LogWarning("Debito de tarifa {IdRequisicaoOrigem} ja foi processado", message.IdRequisicaoOrigem);
                return;
            }

            try
            {
                // Buscar conta
                var conta = await contaRepository.ObterPorNumeroAsync(message.NumeroContaCorrente);
                if (conta == null || conta.Ativo == 0)
                {
                    _logger.LogError("Conta {NumeroConta} nao encontrada ou inativa para debito de tarifa", message.NumeroContaCorrente);
                    return;
                }

                // Criar movimento de debito
                var movimento = new Movimento
                {
                    IdContaCorrente = conta.IdContaCorrente.ToString(),
                    DataMovimento = DateTime.UtcNow,
                    TipoMovimento = "D",
                    Valor = message.ValorTarifa
                };

                await movimentoRepository.AdicionarMovimentoAsync(movimento);

                // Salvar idempotencia
                await idempotenciaRepository.SalvarAsync(new Idempotencia
                {
                    ChaveIdempotencia = chaveIdempotencia,
                    Requisicao = $"Tarifa:{message.NumeroContaCorrente}:{message.ValorTarifa}",
                    Resultado = "OK"
                });

                _logger.LogInformation("Debito de tarifa {IdRequisicaoOrigem} processado com sucesso. Conta: {NumeroConta}, Valor: {ValorTarifa}",
                    message.IdRequisicaoOrigem, message.NumeroContaCorrente, message.ValorTarifa);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar debito de tarifa {IdRequisicaoOrigem}", message.IdRequisicaoOrigem);
                throw;
            }
        }
    }
}
