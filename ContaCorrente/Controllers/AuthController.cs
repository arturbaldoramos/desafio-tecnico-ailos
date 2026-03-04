using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContaCorrente.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        /// <summary>
        /// Valida o token JWT e retorna as informações do usuário autenticado.
        /// Este endpoint é usado por outros serviços para validar tokens.
        /// </summary>
        [HttpGet("validate")]
        [Authorize]
        public IActionResult ValidateToken()
        {
            var idContaCorrente = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var numeroConta = User.FindFirst("numeroConta")?.Value;
            var nome = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(idContaCorrente) || string.IsNullOrEmpty(numeroConta))
            {
                return Unauthorized(new { message = "Token inválido" });
            }

            return Ok(new TokenValidationResponse
            {
                IsValid = true,
                IdContaCorrente = int.Parse(idContaCorrente),
                NumeroConta = numeroConta,
                Nome = nome ?? string.Empty
            });
        }
    }

    public class TokenValidationResponse
    {
        public bool IsValid { get; set; }
        public int IdContaCorrente { get; set; }
        public string NumeroConta { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
    }
}
