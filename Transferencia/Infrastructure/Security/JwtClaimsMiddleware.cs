using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Transferencia.Infrastructure.Security
{
    public class JwtClaimsMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtClaimsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();

                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(token);

                    var identity = new ClaimsIdentity(jwtToken.Claims, "Gateway");
                    context.User = new ClaimsPrincipal(identity);
                }
                catch
                {
                    // Token inválido — prossegue sem autenticar
                }
            }

            await _next(context);
        }
    }
}
