using ContaCorrente.Domain.Entities;
using ContaCorrente.Domain.Interfaces;
using Dapper;
using Microsoft.Data.Sqlite;
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

        private IDbConnection CreateConnection() => new SqliteConnection(_connectionString);

        public async Task AdicionarMovimentoAsync(Movimento movimento)
        {
            using var db = CreateConnection();
            var sql = @"INSERT INTO movimento (idcontacorrente, datamovimento, tipomovimento, valor)
                        VALUES (@IdContaCorrente, @DataMovimento, @TipoMovimento, @Valor);";
            await db.ExecuteAsync(sql, movimento);
        }

        public async Task<IEnumerable<Movimento>> ObterMovimentosPorContaAsync(int idContaCorrente)
        {
            using var db = CreateConnection();
            var sql = @"SELECT idmovimento, idcontacorrente, datamovimento, tipomovimento, valor
                        FROM movimento
                        WHERE idcontacorrente = @IdContaCorrente
                        ORDER BY datamovimento DESC";
            return await db.QueryAsync<Movimento>(sql, new { IdContaCorrente = idContaCorrente });
        }
    }
}
