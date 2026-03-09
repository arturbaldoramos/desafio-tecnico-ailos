using Dapper;
using Npgsql;
using System.Data;
using Tarifa.Domain.Entities;
using Tarifa.Domain.Interfaces;

namespace Tarifa.Infrastructure.Data
{
    public class IdempotenciaRepository : IIdempotenciaRepository
    {
        private readonly string _connectionString;

        public IdempotenciaRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

        public async Task<Idempotencia?> ObterPorChaveAsync(string chave)
        {
            using var db = CreateConnection();
            var sql = @"SELECT chave_idempotencia AS ChaveIdempotencia,
                               requisicao AS Requisicao,
                               resultado AS Resultado
                        FROM idempotencia
                        WHERE chave_idempotencia = @Chave";

            return await db.QueryFirstOrDefaultAsync<Idempotencia>(sql, new { Chave = chave });
        }

        public async Task SalvarAsync(Idempotencia idempotencia)
        {
            using var db = CreateConnection();
            var sql = @"INSERT INTO idempotencia (chave_idempotencia, requisicao, resultado)
                        VALUES (@ChaveIdempotencia, @Requisicao, @Resultado)
                        ON CONFLICT (chave_idempotencia) DO UPDATE
                        SET requisicao = EXCLUDED.requisicao, resultado = EXCLUDED.resultado";

            await db.ExecuteAsync(sql, idempotencia);
        }
    }
}
