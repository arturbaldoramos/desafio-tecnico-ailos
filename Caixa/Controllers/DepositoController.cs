using Caixa.Application.Commands.RealizarDeposito;
using Caixa.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Caixa.Controllers
{
    [ApiController]
    [Route("api/caixa/deposito")]
    public class DepositoController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DepositoController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(DepositoResponse), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IResult> Depositar([FromBody] DepositoRequest request)
        {
            var numeroConta = User.FindFirst("numeroConta")?.Value;

            if (string.IsNullOrEmpty(numeroConta))
                return Results.Unauthorized();

            var command = new RealizarDepositoCommand(
                request.IdRequisicao,
                numeroConta,
                request.Valor
            );

            return await _mediator.Send(command);
        }
    }
}
