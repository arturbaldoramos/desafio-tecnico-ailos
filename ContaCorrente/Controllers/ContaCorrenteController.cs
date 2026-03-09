using ContaCorrente.Application.DTOs;
using ContaCorrente.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ContaCorrente.Controller
{
    [ApiController]
    [Route("api/contacorrente")]
    public class ContaCorrenteController : ControllerBase
    {
        private readonly IContaRepository _contaRepository;

        public ContaCorrenteController(IContaRepository contaRepository)
        {
            _contaRepository = contaRepository;
        }

        /// <summary>
        /// Busca uma conta pelo número (usado por outros serviços)
        /// </summary>
        [HttpGet("{numeroConta}")]
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

            return Ok(new ContaResponse(conta.Numero, conta.Nome));
        }
    }
}
