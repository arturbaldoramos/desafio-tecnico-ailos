using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Transferencia.Infrastructure.Services;

namespace Transferencia.Infrastructure.Security
{
    public class ContaCorrenteAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IContaCorrenteApiClient _apiClient;

        public ContaCorrenteAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IContaCorrenteApiClient apiClient)
            : base(options, logger, encoder)
        {
            _apiClient = apiClient;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return AuthenticateResult.NoResult();
            }

            var authHeader = Request.Headers["Authorization"].ToString();
            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return AuthenticateResult.NoResult();
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            if (string.IsNullOrEmpty(token))
            {
                return AuthenticateResult.Fail("Token não fornecido");
            }

            var validationResult = await _apiClient.ValidateTokenAsync(token);
            if (validationResult == null || !validationResult.IsValid)
            {
                return AuthenticateResult.Fail("Token inválido ou expirado");
            }

            // Criar claims baseado na resposta da API ContaCorrente
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, validationResult.IdContaCorrente.ToString()),
                new Claim("numeroConta", validationResult.NumeroConta),
                new Claim(ClaimTypes.Name, validationResult.Nome)
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}
