namespace Autenticacao.Application.DTOs
{
    public record LoginResponse(string Token, string NumeroConta, string Nome);
}
