using ContaCorrente.Domain.Entities;
using ContaCorrente.Domain.Interfaces;
using ContaCorrente.Infrastructure.Messaging.Messages;
using KafkaFlow;

namespace ContaCorrente.Infrastructure.Messaging.Consumers
{
    public class UsuarioInativadoConsumer : IMessageHandler<UsuarioInativadoMessage>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UsuarioInativadoConsumer> _logger;

        public UsuarioInativadoConsumer(IServiceProvider serviceProvider, ILogger<UsuarioInativadoConsumer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task Handle(IMessageContext context, UsuarioInativadoMessage message)
        {
            _logger.LogInformation("Processando evento usuario-inativado: NumeroConta={NumeroConta}", message.NumeroConta);

            using var scope = _serviceProvider.CreateScope();
            var contaRepository = scope.ServiceProvider.GetRequiredService<IContaRepository>();
            var idempotenciaRepository = scope.ServiceProvider.GetRequiredService<IIdempotenciaRepository>();

            var chaveIdempotencia = $"USR-INATIVACAO-{message.IdUsuario}";

            var idempotencia = await idempotenciaRepository.ObterPorChaveAsync(chaveIdempotencia);
            if (idempotencia != null)
            {
                _logger.LogWarning("Inativação do usuário {IdUsuario} já foi processada", message.IdUsuario);
                return;
            }

            try
            {
                await contaRepository.InativarContaAsync(message.NumeroConta);

                await idempotenciaRepository.SalvarAsync(new Idempotencia
                {
                    ChaveIdempotencia = chaveIdempotencia,
                    Requisicao = $"InativarConta:{message.NumeroConta}",
                    Resultado = "OK"
                });

                _logger.LogInformation("Conta {NumeroConta} inativada com sucesso", message.NumeroConta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inativar conta {NumeroConta}", message.NumeroConta);
                throw;
            }
        }
    }
}
