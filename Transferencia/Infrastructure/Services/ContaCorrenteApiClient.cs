using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Transferencia.Infrastructure.Services
{
    public interface IContaCorrenteApiClient
    {
        Task<ContaResult?> ObterContaPorNumeroAsync(string numeroConta, string token);
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
    }

    public class ContaResult
    {
        public int IdContaCorrente { get; set; }
        public string NumeroConta { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
    }
}
