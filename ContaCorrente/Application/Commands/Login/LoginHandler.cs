using ContaCorrente.Application.DTOs;
using ContaCorrente.Application.Validators;
using ContaCorrente.Domain.Interfaces;
using ContaCorrente.Infrastructure.Security;
using MediatR;

namespace ContaCorrente.Application.Commands.Login
{
    public class LoginHandler : IRequestHandler<LoginCommand, IResult>
    {
        private readonly IContaRepository _repository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtService _jwtService;

        public LoginHandler(
            IContaRepository repository,
            IPasswordHasher passwordHasher,
            IJwtService jwtService)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
        }

        public async Task<IResult> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            // Tenta buscar a conta por CPF ou Número
            var conta = await BuscarContaAsync(request.CpfOuNumero);

            if (conta == null)
            {
                return Results.Unauthorized();
            }

            // Verifica se a conta está ativa
            if (conta.Ativo != 1)
            {
                return Results.BadRequest(new ErrorResponse("Conta inativa", "INACTIVE_ACCOUNT"));
            }

            // Verifica a senha
            if (!_passwordHasher.Verify(request.Senha, conta.Senha))
            {
                return Results.Unauthorized();
            }

            // Gera o token JWT
            var token = _jwtService.GenerateToken(
                conta.IdContaCorrente,
                conta.Numero,
                conta.Nome
            );

            return Results.Ok(new LoginResponse(token, conta.Numero, conta.Nome));
        }

        private async Task<Domain.Entities.ContaCorrente?> BuscarContaAsync(string cpfOuNumero)
        {
            // Se parece com CPF (11 dígitos numéricos), busca por CPF
            var apenasDigitos = new string(cpfOuNumero.Where(char.IsDigit).ToArray());

            if (apenasDigitos.Length == 11)
            {
                return await _repository.ObterPorCpfAsync(apenasDigitos);
            }

            // Caso contrário, busca por número da conta
            return await _repository.ObterPorNumeroAsync(cpfOuNumero);
        }
    }
}
