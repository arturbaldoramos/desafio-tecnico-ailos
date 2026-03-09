using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Autenticacao.Infrastructure.Security
{
    public interface IJwtService
    {
        string GenerateToken(int id, string numeroConta, string cpf, string nome);
    }

    public class JwtService : IJwtService
    {
        private readonly JwtSettings _settings;
        private readonly IRsaKeyService _rsaKeyService;

        public JwtService(IOptions<JwtSettings> settings, IRsaKeyService rsaKeyService)
        {
            _settings = settings.Value;
            _rsaKeyService = rsaKeyService;
        }

        public string GenerateToken(int id, string numeroConta, string cpf, string nome)
        {
            var rsaKey = _rsaKeyService.GetPrivateKey();
            var credentials = new SigningCredentials(
                new RsaSecurityKey(rsaKey), SecurityAlgorithms.RsaSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim("numeroConta", numeroConta),
                new Claim("cpf", cpf),
                new Claim(ClaimTypes.Name, nome),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
