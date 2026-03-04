using ContaCorrente.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ContaCorrente.Infrastructure.Cache
{
    public class SaldoCacheService : ISaldoCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<SaldoCacheService> _logger;
        private readonly TimeSpan _expiracao = TimeSpan.FromMinutes(5);

        public SaldoCacheService(IMemoryCache cache, ILogger<SaldoCacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public Task<decimal?> ObterSaldoAsync(string numeroConta)
        {
            var chave = GerarChave(numeroConta);

            if (_cache.TryGetValue(chave, out decimal saldo))
            {
                return Task.FromResult<decimal?>(saldo);
            }

            return Task.FromResult<decimal?>(null);
        }

        public Task DefinirSaldoAsync(string numeroConta, decimal saldo)
        {
            var chave = GerarChave(numeroConta);

            var opcoes = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(_expiracao)
                .SetPriority(CacheItemPriority.Normal);

            _cache.Set(chave, saldo, opcoes);

            return Task.CompletedTask;
        }

        public void InvalidarSaldo(string numeroConta)
        {
            var chave = GerarChave(numeroConta);
            _cache.Remove(chave);
            _logger.LogDebug("Cache invalidado para conta {NumeroConta}", numeroConta);
        }

        private static string GerarChave(string numeroConta) => $"saldo:{numeroConta}";
    }
}
