using Caixa.Domain.Interfaces;
using Caixa.Infrastructure.Messaging.Messages;
using KafkaFlow;

namespace Caixa.Infrastructure.Messaging.Consumers
{
    public class DepositoResultadoConsumer : IMessageHandler<DepositoResultadoMessage>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DepositoResultadoConsumer> _logger;

        public DepositoResultadoConsumer(IServiceProvider serviceProvider, ILogger<DepositoResultadoConsumer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task Handle(IMessageContext context, DepositoResultadoMessage message)
        {
            _logger.LogInformation("Recebido resultado de depósito {IdRequisicao}: Sucesso={Sucesso}",
                message.IdRequisicao, message.Sucesso);

            using var scope = _serviceProvider.CreateScope();
            var depositoRepository = scope.ServiceProvider.GetRequiredService<IDepositoRepository>();

            var status = message.Sucesso ? "SUCESSO" : "ERRO";
            await depositoRepository.AtualizarStatusAsync(message.IdRequisicao, status, message.MensagemErro);

            _logger.LogInformation("Depósito {IdRequisicao} atualizado para status {Status}", message.IdRequisicao, status);
        }
    }
}
