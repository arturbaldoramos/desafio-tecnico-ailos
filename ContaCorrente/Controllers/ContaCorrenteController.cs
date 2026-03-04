using ContaCorrente.Application.Commands.CadastrarConta;
using ContaCorrente.Application.Commands.InativarConta;
using ContaCorrente.Application.Commands.Login;
using ContaCorrente.Application.DTOs;
using ContaCorrente.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContaCorrente.Controller
{
    [ApiController]
    [Route("api/contacorrente")]
    public class ContaCorrenteController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IContaRepository _contaRepository;

        public ContaCorrenteController(IMediator mediator, IContaRepository contaRepository)
        {
            _mediator = mediator;
            _contaRepository = contaRepository;
        }

        /// <summary>
        /// Cadastra uma nova conta corrente
        /// </summary>
        [HttpPost("cadastrar")]
        [ProducesResponseType(typeof(CadastrarContaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IResult> Cadastrar([FromBody] CadastrarContaRequest request)
        {
            var command = new CadastrarContaCommand(request.Nome, request.Cpf, request.Senha);
            return await _mediator.Send(command);
        }

        /// <summary>
        /// Realiza login e retorna token JWT
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IResult> Login([FromBody] LoginRequest request)
        {
            var command = new LoginCommand(request.CpfOuNumero, request.Senha);
            return await _mediator.Send(command);
        }

        /// <summary>
        /// Inativa uma conta corrente (requer autenticação)
        /// </summary>
        [HttpDelete("inativar")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IResult> Inativar([FromBody] InativarContaRequest request)
        {
            var command = new InativarContaCommand(request.NumeroConta, request.Senha);
            return await _mediator.Send(command);
        }

        /// <summary>
        /// Busca uma conta pelo número (usado por outros serviços)
        /// </summary>
        [HttpGet("{numeroConta}")]
        [Authorize]
        [ProducesResponseType(typeof(ContaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ObterPorNumero(string numeroConta)
        {
            var conta = await _contaRepository.ObterPorNumeroAsync(numeroConta);
            if (conta == null || conta.Ativo == 0)
            {
                return NotFound();
            }

            return Ok(new ContaResponse(conta.IdContaCorrente, conta.Numero, conta.Nome));
        }
    }
}
