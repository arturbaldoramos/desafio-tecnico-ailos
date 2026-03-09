using Autenticacao.Application.DTOs;
using Autenticacao.Domain.Entities;
using Autenticacao.Domain.Interfaces;
using Autenticacao.Infrastructure.Security;
using MediatR;

namespace Autenticacao.Application.Commands.Login
{
    public class LoginHandler : IRequestHandler<LoginCommand, IResult>
    {
        private readonly IUsuarioRepository _repository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtService _jwtService;

        public LoginHandler(
            IUsuarioRepository repository,
            IPasswordHasher passwordHasher,
            IJwtService jwtService)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
        }

        public async Task<IResult> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var usuario = await BuscarUsuarioAsync(request.CpfOuNumero);

            if (usuario == null)
            {
                return Results.Unauthorized();
            }

            if (usuario.Ativo != 1)
            {
                return Results.BadRequest(new ErrorResponse("Conta inativa", "INACTIVE_ACCOUNT"));
            }

            if (!_passwordHasher.Verify(request.Senha, usuario.SenhaHash))
            {
                return Results.Unauthorized();
            }

            var token = _jwtService.GenerateToken(
                usuario.Id,
                usuario.NumeroConta,
                usuario.Cpf,
                usuario.Nome
            );

            return Results.Ok(new LoginResponse(token, usuario.NumeroConta, usuario.Nome));
        }

        private async Task<Usuario?> BuscarUsuarioAsync(string cpfOuNumero)
        {
            var apenasDigitos = new string(cpfOuNumero.Where(char.IsDigit).ToArray());

            if (apenasDigitos.Length == 11)
            {
                return await _repository.ObterPorCpfAsync(apenasDigitos);
            }

            return await _repository.ObterPorNumeroContaAsync(cpfOuNumero);
        }
    }
}
