using Dapper;
using Microsoft.Data.Sqlite;
using System.Data;
using Transferencia.Domain.Entities;
using Transferencia.Domain.Interfaces;

namespace Transferencia.Infrastructure.Data
{
    public class TransferenciaRepository : ITransferenciaRepository
    {
        private readonly string _connectionString;

        public TransferenciaRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        private IDbConnection CreateConnection() => new SqliteConnection(_connectionString);

        public async Task AdicionarAsync(Domain.Entities.Transferencia transferencia)
        {
            using var db = CreateConnection();
            var sql = @"INSERT INTO transferencia
                        (idrequisicao, numerocontaorigem, numerocontadestino, datamovimento, valor, status, mensagemerro)
                        VALUES (@IdRequisicao, @NumeroContaOrigem, @NumeroContaDestino, @DataMovimento, @Valor, @Status, @MensagemErro);
                        SELECT last_insert_rowid();";

            transferencia.IdTransferencia = await db.ExecuteScalarAsync<int>(sql, transferencia);
        }

        public async Task<Domain.Entities.Transferencia?> ObterPorIdAsync(int id)
        {
            using var db = CreateConnection();
            var sql = @"SELECT idtransferencia AS IdTransferencia,
                               idrequisicao AS IdRequisicao,
                               numerocontaorigem AS NumeroContaOrigem,
                               numerocontadestino AS NumeroContaDestino,
                               datamovimento AS DataMovimento,
                               valor AS Valor,
                               status AS Status,
                               mensagemerro AS MensagemErro
                        FROM transferencia
                        WHERE idtransferencia = @Id";

            return await db.QueryFirstOrDefaultAsync<Domain.Entities.Transferencia>(sql, new { Id = id });
        }

        public async Task<Domain.Entities.Transferencia?> ObterPorIdRequisicaoAsync(string idRequisicao)
        {
            using var db = CreateConnection();
            var sql = @"SELECT idtransferencia AS IdTransferencia,
                               idrequisicao AS IdRequisicao,
                               numerocontaorigem AS NumeroContaOrigem,
                               numerocontadestino AS NumeroContaDestino,
                               datamovimento AS DataMovimento,
                               valor AS Valor,
                               status AS Status,
                               mensagemerro AS MensagemErro
                        FROM transferencia
                        WHERE idrequisicao = @IdRequisicao";

            return await db.QueryFirstOrDefaultAsync<Domain.Entities.Transferencia>(sql, new { IdRequisicao = idRequisicao });
        }

        public async Task AtualizarStatusAsync(string idRequisicao, string status, string? mensagemErro = null)
        {
            using var db = CreateConnection();
            var sql = @"UPDATE transferencia
                        SET status = @Status, mensagemerro = @MensagemErro
                        WHERE idrequisicao = @IdRequisicao";

            await db.ExecuteAsync(sql, new { IdRequisicao = idRequisicao, Status = status, MensagemErro = mensagemErro });
        }
    }
}
