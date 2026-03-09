using Autenticacao.Application.DTOs;
using Autenticacao.Domain.Interfaces;
using Autenticacao.Infrastructure.Messaging;
using Autenticacao.Infrastructure.Messaging.Messages;
using Autenticacao.Infrastructure.Security;
using KafkaFlow.Producers;
using MediatR;

namespace Autenticacao.Application.Commands.InativarUsuario
{
    public class InativarUsuarioHandler : IRequestHandler<InativarUsuarioCommand, IResult>
    {
        private readonly IUsuarioRepository _repository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IProducerAccessor _producerAccessor;

        public InativarUsuarioHandler(
            IUsuarioRepository repository,
            IPasswordHasher passwordHasher,
            IProducerAccessor producerAccessor)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
            _producerAccessor = producerAccessor;
        }

        public async Task<IResult> Handle(InativarUsuarioCommand request, CancellationToken cancellationToken)
        {
            var usuario = await _repository.ObterPorNumeroContaAsync(request.NumeroConta);

            if (usuario == null)
            {
                return Results.BadRequest(new ErrorResponse("Conta não encontrada", "INVALID_ACCOUNT"));
            }

            if (usuario.Ativo == 0)
            {
                return Results.BadRequest(new ErrorResponse("Conta já está inativa", "INACTIVE_ACCOUNT"));
            }

            if (!_passwordHasher.Verify(request.Senha, usuario.SenhaHash))
            {
                return Results.Json(new ErrorResponse("Credenciais inválidas", "USER_UNAUTHORIZED"), statusCode: 401);
            }

            await _repository.InativarAsync(request.NumeroConta);

            var message = new UsuarioInativadoMessage
            {
                IdUsuario = usuario.Id,
                NumeroConta = request.NumeroConta,
                DataInativacao = DateTime.UtcNow
            };

            var producer = _producerAccessor.GetProducer("usuario-inativado-producer");
            await producer.ProduceAsync(KafkaTopics.UsuarioInativado, request.NumeroConta, message);

            return Results.NoContent();
        }
    }
}
