using Autenticacao.Application.Commands.CadastrarUsuario;
using Autenticacao.Application.Commands.InativarUsuario;
using Autenticacao.Application.Commands.Login;
using Autenticacao.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Autenticacao.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("cadastrar")]
        [ProducesResponseType(typeof(CadastrarUsuarioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IResult> Cadastrar([FromBody] CadastrarUsuarioRequest request)
        {
            var command = new CadastrarUsuarioCommand(request.Nome, request.Cpf, request.Senha);
            return await _mediator.Send(command);
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IResult> Login([FromBody] LoginRequest request)
        {
            var command = new LoginCommand(request.CpfOuNumero, request.Senha);
            return await _mediator.Send(command);
        }

        [HttpGet("validate")]
        [Authorize]
        public IActionResult ValidateToken()
        {
            var idUsuario = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var numeroConta = User.FindFirst("numeroConta")?.Value;
            var nome = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            var cpf = User.FindFirst("cpf")?.Value;

            if (string.IsNullOrEmpty(idUsuario) || string.IsNullOrEmpty(numeroConta))
            {
                return Unauthorized(new { message = "Token inválido" });
            }

            return Ok(new
            {
                IsValid = true,
                IdUsuario = int.Parse(idUsuario),
                NumeroConta = numeroConta,
                Nome = nome ?? string.Empty,
                Cpf = cpf ?? string.Empty
            });
        }

        [HttpDelete("inativar")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IResult> Inativar([FromBody] InativarUsuarioRequest request)
        {
            var command = new InativarUsuarioCommand(request.NumeroConta, request.Senha);
            return await _mediator.Send(command);
        }
    }
}
