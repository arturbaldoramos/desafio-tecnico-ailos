using ContaCorrente.Application.DTOs;
using ContaCorrente.Domain.Interfaces;
using ContaCorrente.Infrastructure.Security;
using MediatR;

namespace ContaCorrente.Application.Commands.InativarConta
{
    public class InativarContaHandler : IRequestHandler<InativarContaCommand, IResult>
    {
        private readonly IContaRepository _repository;
        private readonly IPasswordHasher _passwordHasher;

        public InativarContaHandler(IContaRepository repository, IPasswordHasher passwordHasher)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
        }

        public async Task<IResult> Handle(InativarContaCommand request, CancellationToken cancellationToken)
        {
            var conta = await _repository.ObterPorNumeroAsync(request.NumeroConta);

            if (conta == null)
            {
                return Results.BadRequest(new ErrorResponse("Conta não encontrada", "INVALID_ACCOUNT"));
            }

            if (conta.Ativo == 0)
            {
                return Results.BadRequest(new ErrorResponse("Conta já está inativa", "INACTIVE_ACCOUNT"));
            }

            if (!_passwordHasher.Verify(request.Senha, conta.Senha))
            {
                return Results.Json(new ErrorResponse("Credenciais inválidas", "USER_UNAUTHORIZED"), statusCode: 401);
            }

            await _repository.InativarContaAsync(request.NumeroConta);

            return Results.NoContent();
        }
    }
}
