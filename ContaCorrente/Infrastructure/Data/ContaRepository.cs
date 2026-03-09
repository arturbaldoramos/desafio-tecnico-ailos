using ContaCorrente.Domain.Entities;
using ContaCorrente.Domain.Interfaces;
using Dapper;
using Npgsql;
using System.Data;

namespace ContaCorrente.Infrastructure.Data
{
    public class ContaRepository : IContaRepository
    {
        private readonly string _connectionString;

        public ContaRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

        public async Task<int> AdicionarContaAsync(Domain.Entities.ContaCorrente conta)
        {
            using var db = CreateConnection();
            var sql = @"INSERT INTO contacorrente (numero, nome, cpf, ativo)
                        VALUES (@Numero, @Nome, @Cpf, @Ativo)
                        RETURNING id;";
            return await db.QuerySingleAsync<int>(sql, conta);
        }

        public async Task<Domain.Entities.ContaCorrente?> ObterPorCpfAsync(string cpf)
        {
            using var db = CreateConnection();
            var sql = "SELECT id, numero, nome, cpf, ativo FROM contacorrente WHERE cpf = @Cpf";
            return await db.QueryFirstOrDefaultAsync<Domain.Entities.ContaCorrente>(sql, new { Cpf = cpf });
        }

        public async Task<Domain.Entities.ContaCorrente?> ObterPorNumeroAsync(string numero)
        {
            using var db = CreateConnection();
            var sql = "SELECT id, numero, nome, cpf, ativo FROM contacorrente WHERE numero = @Numero";
            return await db.QueryFirstOrDefaultAsync<Domain.Entities.ContaCorrente>(sql, new { Numero = numero });
        }

        public async Task InativarContaAsync(string numero)
        {
            using var db = CreateConnection();
            var sql = "UPDATE contacorrente SET ativo = 0 WHERE numero = @Numero";
            await db.ExecuteAsync(sql, new { Numero = numero });
        }
    }
}
