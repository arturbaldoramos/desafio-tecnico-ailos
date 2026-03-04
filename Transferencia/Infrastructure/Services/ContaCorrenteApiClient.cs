using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Transferencia.Infrastructure.Services
{
    public interface IContaCorrenteApiClient
    {
        Task<TokenValidationResult?> ValidateTokenAsync(string token);
        Task<ContaResult?> ObterContaPorNumeroAsync(string numeroConta, string token);
        Task<bool> CriarMovimentacaoAsync(string numeroConta, string tipo, decimal valor, string idRequisicao, string token);
    }

    public class ContaCorrenteApiClient : IContaCorrenteApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ContaCorrenteApiClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public ContaCorrenteApiClient(HttpClient httpClient, ILogger<ContaCorrenteApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<TokenValidationResult?> ValidateTokenAsync(string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "api/auth/validate");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Token validation failed with status code: {StatusCode}", response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TokenValidationResult>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token with ContaCorrente API");
                return null;
            }
        }

        public async Task<ContaResult?> ObterContaPorNumeroAsync(string numeroConta, string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"api/contacorrente/{numeroConta}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Conta não encontrada: {NumeroConta}, StatusCode: {StatusCode}", numeroConta, response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ContaResult>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching conta from ContaCorrente API");
                return null;
            }
        }

        public async Task<bool> CriarMovimentacaoAsync(string numeroConta, string tipo, decimal valor, string idRequisicao, string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "api/movimentacao");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var body = new
                {
                    idRequisicao,
                    tipo,
                    valor
                };

                request.Content = new StringContent(
                    JsonSerializer.Serialize(body),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Falha ao criar movimentação: {StatusCode} - {Error}", response.StatusCode, errorContent);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating movimentacao in ContaCorrente API");
                return false;
            }
        }
    }

    public class TokenValidationResult
    {
        public bool IsValid { get; set; }
        public int IdContaCorrente { get; set; }
        public string NumeroConta { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
    }

    public class ContaResult
    {
        public int IdContaCorrente { get; set; }
        public string NumeroConta { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
    }
}
