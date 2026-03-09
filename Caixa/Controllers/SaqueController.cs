using Caixa.Application.Commands.RealizarSaque;
using Caixa.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Caixa.Controllers
{
    [ApiController]
    [Route("api/caixa/saque")]
    public class SaqueController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SaqueController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(SaqueResponse), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IResult> Sacar([FromBody] SaqueRequest request)
        {
            var numeroConta = User.FindFirst("numeroConta")?.Value;

            if (string.IsNullOrEmpty(numeroConta))
                return Results.Unauthorized();

            var command = new RealizarSaqueCommand(
                request.IdRequisicao,
                numeroConta,
                request.Valor
            );

            return await _mediator.Send(command);
        }
    }
}
