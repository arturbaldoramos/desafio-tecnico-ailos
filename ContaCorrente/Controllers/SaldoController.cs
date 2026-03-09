using ContaCorrente.Application.DTOs;
using ContaCorrente.Application.Queries.ConsultaSaldo;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ContaCorrente.Controller
{
    [ApiController]
    [Route("api/saldo")]
    public class SaldoController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SaldoController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(SaldoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IResult> ObterSaldo()
        {
            var numeroConta = User.FindFirst("numeroConta")?.Value;

            if (string.IsNullOrEmpty(numeroConta))
            {
                return Results.Unauthorized();
            }

            var query = new ObterSaldoQuery(numeroConta);
            return await _mediator.Send(query);
        }
    }
}
