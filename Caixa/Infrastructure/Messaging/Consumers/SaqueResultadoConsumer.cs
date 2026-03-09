using Caixa.Domain.Interfaces;
using Caixa.Infrastructure.Messaging.Messages;
using KafkaFlow;

namespace Caixa.Infrastructure.Messaging.Consumers
{
    public class SaqueResultadoConsumer : IMessageHandler<SaqueResultadoMessage>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SaqueResultadoConsumer> _logger;

        public SaqueResultadoConsumer(IServiceProvider serviceProvider, ILogger<SaqueResultadoConsumer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task Handle(IMessageContext context, SaqueResultadoMessage message)
        {
            _logger.LogInformation("Recebido resultado de saque {IdRequisicao}: Sucesso={Sucesso}",
                message.IdRequisicao, message.Sucesso);

            using var scope = _serviceProvider.CreateScope();
            var saqueRepository = scope.ServiceProvider.GetRequiredService<ISaqueRepository>();

            var status = message.Sucesso ? "SUCESSO" : "ERRO";
            await saqueRepository.AtualizarStatusAsync(message.IdRequisicao, status, message.MensagemErro);

            _logger.LogInformation("Saque {IdRequisicao} atualizado para status {Status}", message.IdRequisicao, status);
        }
    }
}
