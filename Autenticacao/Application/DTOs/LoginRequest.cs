namespace Autenticacao.Application.DTOs
{
    public record LoginRequest(string CpfOuNumero, string Senha);
}
