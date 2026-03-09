using Dapper;
using Caixa.Domain.Interfaces;
using Npgsql;
using System.Data;

namespace Caixa.Infrastructure.Data
{
    public class DepositoRepository : IDepositoRepository
    {
        private readonly string _connectionString;

        public DepositoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

        public async Task AdicionarAsync(Domain.Entities.Deposito deposito)
        {
            using var db = CreateConnection();
            var sql = @"INSERT INTO deposito (idrequisicao, numeroconta, valor, datadeposito, status)
                        VALUES (@IdRequisicao, @NumeroConta, @Valor, @DataDeposito, @Status);";
            await db.ExecuteAsync(sql, deposito);
        }

        public async Task<Domain.Entities.Deposito?> ObterPorIdRequisicaoAsync(string idRequisicao)
        {
            using var db = CreateConnection();
            var sql = @"SELECT id, idrequisicao AS IdRequisicao, numeroconta AS NumeroConta, valor,
                               datadeposito AS DataDeposito, status, mensagemerro AS MensagemErro
                        FROM deposito
                        WHERE idrequisicao = @IdRequisicao";
            return await db.QueryFirstOrDefaultAsync<Domain.Entities.Deposito>(sql, new { IdRequisicao = idRequisicao });
        }

        public async Task AtualizarStatusAsync(string idRequisicao, string status, string? mensagemErro = null)
        {
            using var db = CreateConnection();
            var sql = @"UPDATE deposito SET status = @Status, mensagemerro = @MensagemErro
                        WHERE idrequisicao = @IdRequisicao";
            await db.ExecuteAsync(sql, new { IdRequisicao = idRequisicao, Status = status, MensagemErro = mensagemErro });
        }
    }
}
