using Autenticacao.Domain.Entities;
using Autenticacao.Domain.Interfaces;
using Dapper;
using Npgsql;
using System.Data;

namespace Autenticacao.Infrastructure.Data
{
    public class IdempotenciaRepository : IIdempotenciaRepository
    {
        private readonly string _connectionString;

        public IdempotenciaRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

        public async Task<Idempotencia?> ObterPorChaveAsync(string chave)
        {
            using var db = CreateConnection();
            var sql = "SELECT chave_idempotencia AS ChaveIdempotencia, requisicao, resultado FROM idempotencia WHERE chave_idempotencia = @Chave";
            return await db.QueryFirstOrDefaultAsync<Idempotencia>(sql, new { Chave = chave });
        }

        public async Task SalvarAsync(Idempotencia idempotencia)
        {
            using var db = CreateConnection();
            var sql = @"INSERT INTO idempotencia (chave_idempotencia, requisicao, resultado)
                        VALUES (@ChaveIdempotencia, @Requisicao, @Resultado);";
            await db.ExecuteAsync(sql, idempotencia);
        }
    }
}
