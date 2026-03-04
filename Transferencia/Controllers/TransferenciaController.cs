using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Transferencia.Application.Commands.RealizarTransferencia;
using Transferencia.Application.DTOs;
using Transferencia.Domain.Interfaces;

namespace Transferencia.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransferenciaController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ITransferenciaRepository _transferenciaRepository;

        public TransferenciaController(IMediator mediator, ITransferenciaRepository transferenciaRepository)
        {
            _mediator = mediator;
            _transferenciaRepository = transferenciaRepository;
        }

        /// <summary>
        /// Realiza uma transferência entre contas (assíncrono via Kafka)
        /// </summary>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(TransferenciaResponse), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IResult> RealizarTransferencia([FromBody] TransferenciaRequest request)
        {
            var numeroConta = User.FindFirst("numeroConta")?.Value;

            if (string.IsNullOrEmpty(numeroConta))
            {
                return Results.Unauthorized();
            }

            var command = new RealizarTransferenciaCommand(
                request.IdRequisicao,
                numeroConta,
                request.ContaDestino,
                request.Valor
            );

            return await _mediator.Send(command);
        }

        /// <summary>
        /// Consulta o status de uma transferência pelo ID da requisição
        /// </summary>
        [HttpGet("{idRequisicao}")]
        [Authorize]
        [ProducesResponseType(typeof(TransferenciaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ConsultarStatus(string idRequisicao)
        {
            var transferencia = await _transferenciaRepository.ObterPorIdRequisicaoAsync(idRequisicao);

            if (transferencia == null)
            {
                return NotFound(new ErrorResponse("Transferência não encontrada", "NOT_FOUND"));
            }

            return Ok(new TransferenciaResponse(
                transferencia.IdTransferencia,
                transferencia.NumeroContaOrigem,
                transferencia.NumeroContaDestino,
                transferencia.Valor,
                transferencia.DataMovimento,
                transferencia.Status
            ));
        }

        /// <summary>
        /// Retorna informações do usuário autenticado
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            var idContaCorrente = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var numeroConta = User.FindFirst("numeroConta")?.Value;
            var nome = User.FindFirst(ClaimTypes.Name)?.Value;

            return Ok(new
            {
                IdContaCorrente = idContaCorrente,
                NumeroConta = numeroConta,
                Nome = nome
            });
        }
    }
}
