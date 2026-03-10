using ContaCorrente.Application.DTOs;
using ContaCorrente.Application.Queries.ConsultaExtrato;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ContaCorrente.Controller
{
    [ApiController]
    [Route("api/contacorrente")]
    public class ExtratoController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ExtratoController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Consulta extrato bancário da conta autenticada
        /// </summary>
        [HttpGet("extrato")]
        [ProducesResponseType(typeof(ExtratoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IResult> ObterExtrato(
            [FromQuery] DateTime dataInicio,
            [FromQuery] DateTime dataFim,
            [FromQuery] string? tipoMovimento = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 20)
        {
            var numeroConta = User.FindFirst("numeroConta")?.Value;

            if (string.IsNullOrEmpty(numeroConta))
                return Results.Unauthorized();

            if (dataInicio > dataFim)
                return Results.BadRequest(new ErrorResponse("Data início não pode ser maior que data fim", "INVALID_DATE_RANGE"));

            if (tipoMovimento != null && tipoMovimento != "C" && tipoMovimento != "D")
                return Results.BadRequest(new ErrorResponse("Tipo de movimento deve ser 'C' ou 'D'", "INVALID_MOVEMENT_TYPE"));

            if (pagina < 1) pagina = 1;
            if (tamanhoPagina < 1) tamanhoPagina = 1;
            if (tamanhoPagina > 100) tamanhoPagina = 100;

            var query = new ConsultaExtratoQuery(numeroConta, dataInicio, dataFim, tipoMovimento, pagina, tamanhoPagina);
            return await _mediator.Send(query);
        }
    }
}
