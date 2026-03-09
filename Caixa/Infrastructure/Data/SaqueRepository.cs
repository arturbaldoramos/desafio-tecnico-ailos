using Dapper;
using Caixa.Domain.Entities;
using Caixa.Domain.Interfaces;
using Npgsql;
using System.Data;

namespace Caixa.Infrastructure.Data
{
    public class SaqueRepository : ISaqueRepository
    {
        private readonly string _connectionString;

        public SaqueRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

        public async Task AdicionarAsync(Saque saque)
        {
            using var db = CreateConnection();
            var sql = @"INSERT INTO saque (idrequisicao, numeroconta, valor, datasaque, status)
                        VALUES (@IdRequisicao, @NumeroConta, @Valor, @DataSaque, @Status);";
            await db.ExecuteAsync(sql, saque);
        }

        public async Task<Saque?> ObterPorIdRequisicaoAsync(string idRequisicao)
        {
            using var db = CreateConnection();
            var sql = @"SELECT id, idrequisicao AS IdRequisicao, numeroconta AS NumeroConta, valor,
                               datasaque AS DataSaque, status, mensagemerro AS MensagemErro
                        FROM saque
                        WHERE idrequisicao = @IdRequisicao";
            return await db.QueryFirstOrDefaultAsync<Saque>(sql, new { IdRequisicao = idRequisicao });
        }

        public async Task AtualizarStatusAsync(string idRequisicao, string status, string? mensagemErro = null)
        {
            using var db = CreateConnection();
            var sql = @"UPDATE saque SET status = @Status, mensagemerro = @MensagemErro
                        WHERE idrequisicao = @IdRequisicao";
            await db.ExecuteAsync(sql, new { IdRequisicao = idRequisicao, Status = status, MensagemErro = mensagemErro });
        }
    }
}
