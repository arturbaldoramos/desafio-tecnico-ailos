using ContaCorrente.Application.DTOs;
using ContaCorrente.Application.Validators;
using ContaCorrente.Domain.Interfaces;
using ContaCorrente.Infrastructure.Security;
using ContaCorrenteEntity = ContaCorrente.Domain.Entities.ContaCorrente;
using MediatR;

namespace ContaCorrente.Application.Commands.CadastrarConta
{
    public class CadastrarContaHandler : IRequestHandler<CadastrarContaCommand, IResult>
    {
        private readonly IContaRepository _repository;
        private readonly IPasswordHasher _passwordHasher;

        public CadastrarContaHandler(IContaRepository repository, IPasswordHasher passwordHasher)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
        }

        public async Task<IResult> Handle(CadastrarContaCommand request, CancellationToken cancellationToken)
        {
            if (!CpfValidator.IsValid(request.Cpf))
            {
                return Results.BadRequest(new ErrorResponse("CPF inválido", "INVALID_DOCUMENT"));
            }

            var contaExistente = await _repository.ObterPorCpfAsync(request.Cpf);
            if (contaExistente != null)
            {
                return Results.BadRequest(new ErrorResponse("CPF já cadastrado", "INVALID_DOCUMENT"));
            }

            var novaConta = new ContaCorrenteEntity
            {
                Nome = request.Nome,
                Cpf = request.Cpf,
                Senha = _passwordHasher.Hash(request.Senha),
                Numero = Guid.NewGuid().ToString().Substring(0, 8),
                Ativo = 1
            };

            await _repository.AdicionarContaAsync(novaConta);

            return Results.Ok(new CadastrarContaResponse(novaConta.Numero));
        }
    }
}
