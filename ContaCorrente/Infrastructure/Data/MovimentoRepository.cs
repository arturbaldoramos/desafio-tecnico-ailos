using ContaCorrente.Domain.Entities;
using ContaCorrente.Domain.Interfaces;
using Dapper;
using Npgsql;
using System.Data;

namespace ContaCorrente.Infrastructure.Data
{
    public class MovimentoRepository : IMovimentoRepository
    {
        private readonly string _connectionString;

        public MovimentoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

        public async Task AdicionarMovimentoAsync(Movimento movimento)
        {
            using var db = CreateConnection();
            var sql = @"INSERT INTO movimento (contacorrente, datamovimento, tipomovimento, valor)
                        VALUES (@ContaCorrente, @DataMovimento, @TipoMovimento, @Valor);";
            await db.ExecuteAsync(sql, movimento);
        }

        public async Task<IEnumerable<Movimento>> ObterMovimentosPorContaAsync(string contaCorrente)
        {
            using var db = CreateConnection();
            var sql = @"SELECT id, contacorrente AS ContaCorrente, datamovimento AS DataMovimento, tipomovimento AS TipoMovimento, valor
                        FROM movimento
                        WHERE contacorrente = @ContaCorrente
                        ORDER BY datamovimento DESC";
            return await db.QueryAsync<Movimento>(sql, new { ContaCorrente = contaCorrente });
        }

        public async Task<decimal> ObterSaldoAsync(string contaCorrente)
        {
            using var db = CreateConnection();
            var sql = @"SELECT COALESCE(
                            SUM(CASE WHEN tipomovimento = 'C' THEN valor ELSE 0 END) -
                            SUM(CASE WHEN tipomovimento = 'D' THEN valor ELSE 0 END), 0)
                        FROM movimento
                        WHERE contacorrente = @ContaCorrente";
            return await db.ExecuteScalarAsync<decimal>(sql, new { ContaCorrente = contaCorrente });
        }
    }
}
