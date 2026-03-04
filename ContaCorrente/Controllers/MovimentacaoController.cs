using System.Security.Claims;
using ContaCorrente.Application.Commands.Movimentacao;
using ContaCorrente.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContaCorrente.Controller
{
    [ApiController]
    [Route("api/movimentacao")]
    [Authorize]
    public class MovimentacaoController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MovimentacaoController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Realiza uma movimentação (crédito ou débito) na conta
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IResult> Movimentar([FromBody] MovimentacaoRequest request)
        {
            var numeroConta = User.FindFirst("numeroConta")?.Value;

            if (string.IsNullOrEmpty(numeroConta))
            {
                return Results.Unauthorized();
            }

            var command = new MovimentacaoCommand(
                request.IdRequisicao,
                numeroConta,
                request.Tipo,
                request.Valor
            );

            return await _mediator.Send(command);
        }
    }
}
