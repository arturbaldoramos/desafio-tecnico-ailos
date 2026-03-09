using ContaCorrente.Domain.Entities;
using ContaCorrente.Domain.Interfaces;
using ContaCorrente.Infrastructure.Messaging.Messages;
using KafkaFlow;

namespace ContaCorrente.Infrastructure.Messaging.Consumers
{
    public class UsuarioCadastradoConsumer : IMessageHandler<UsuarioCadastradoMessage>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UsuarioCadastradoConsumer> _logger;

        public UsuarioCadastradoConsumer(IServiceProvider serviceProvider, ILogger<UsuarioCadastradoConsumer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task Handle(IMessageContext context, UsuarioCadastradoMessage message)
        {
            _logger.LogInformation("Processando evento usuario-cadastrado: CPF={Cpf}, NumeroConta={NumeroConta}",
                message.Cpf, message.NumeroConta);

            using var scope = _serviceProvider.CreateScope();
            var contaRepository = scope.ServiceProvider.GetRequiredService<IContaRepository>();
            var idempotenciaRepository = scope.ServiceProvider.GetRequiredService<IIdempotenciaRepository>();

            var chaveIdempotencia = $"USR-CADASTRO-{message.IdUsuario}";

            var idempotencia = await idempotenciaRepository.ObterPorChaveAsync(chaveIdempotencia);
            if (idempotencia != null)
            {
                _logger.LogWarning("Usuário {IdUsuario} já foi processado no ContaCorrente", message.IdUsuario);
                return;
            }

            try
            {
                var contaExistente = await contaRepository.ObterPorCpfAsync(message.Cpf);
                if (contaExistente != null)
                {
                    _logger.LogWarning("Conta já existe para CPF {Cpf}", message.Cpf);
                    return;
                }

                var novaConta = new Domain.Entities.ContaCorrente
                {
                    Numero = message.NumeroConta,
                    Nome = message.Nome,
                    Cpf = message.Cpf,
                    Ativo = 1
                };

                await contaRepository.AdicionarContaAsync(novaConta);

                await idempotenciaRepository.SalvarAsync(new Idempotencia
                {
                    ChaveIdempotencia = chaveIdempotencia,
                    Requisicao = $"CriarConta:{message.Cpf}:{message.NumeroConta}",
                    Resultado = "OK"
                });

                _logger.LogInformation("Conta corrente criada com sucesso para usuário {IdUsuario}, NumeroConta={NumeroConta}",
                    message.IdUsuario, message.NumeroConta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar conta corrente para usuário {IdUsuario}", message.IdUsuario);
                throw;
            }
        }
    }
}
