using Autenticacao.Application.DTOs;
using Autenticacao.Application.Validators;
using Autenticacao.Domain.Entities;
using Autenticacao.Domain.Interfaces;
using Autenticacao.Infrastructure.Messaging;
using Autenticacao.Infrastructure.Messaging.Messages;
using Autenticacao.Infrastructure.Security;
using KafkaFlow.Producers;
using MediatR;

namespace Autenticacao.Application.Commands.CadastrarUsuario
{
    public class CadastrarUsuarioHandler : IRequestHandler<CadastrarUsuarioCommand, IResult>
    {
        private readonly IUsuarioRepository _repository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IProducerAccessor _producerAccessor;

        public CadastrarUsuarioHandler(
            IUsuarioRepository repository,
            IPasswordHasher passwordHasher,
            IProducerAccessor producerAccessor)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
            _producerAccessor = producerAccessor;
        }

        public async Task<IResult> Handle(CadastrarUsuarioCommand request, CancellationToken cancellationToken)
        {
            if (!CpfValidator.IsValid(request.Cpf))
            {
                return Results.BadRequest(new ErrorResponse("CPF inválido", "INVALID_DOCUMENT"));
            }

            var cpfLimpo = new string(request.Cpf.Where(char.IsDigit).ToArray());

            var usuarioExistente = await _repository.ObterPorCpfAsync(cpfLimpo);
            if (usuarioExistente != null)
            {
                return Results.BadRequest(new ErrorResponse("CPF já cadastrado", "INVALID_DOCUMENT"));
            }

            var numeroConta = Guid.NewGuid().ToString().Substring(0, 8);

            var novoUsuario = new Usuario
            {
                Nome = request.Nome,
                Cpf = cpfLimpo,
                SenhaHash = _passwordHasher.Hash(request.Senha),
                NumeroConta = numeroConta,
                Ativo = 1,
                DataCriacao = DateTime.UtcNow
            };

            var idUsuario = await _repository.AdicionarAsync(novoUsuario);
            novoUsuario.Id = idUsuario;

            var message = new UsuarioCadastradoMessage
            {
                IdUsuario = idUsuario,
                Cpf = cpfLimpo,
                Nome = request.Nome,
                NumeroConta = numeroConta,
                DataCriacao = novoUsuario.DataCriacao
            };

            var producer = _producerAccessor.GetProducer("usuario-cadastrado-producer");
            await producer.ProduceAsync(KafkaTopics.UsuarioCadastrado, cpfLimpo, message);

            return Results.Ok(new CadastrarUsuarioResponse(numeroConta));
        }
    }
}
