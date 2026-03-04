using Dapper;
using Microsoft.Data.Sqlite;
using System.Data;
using Tarifa.Domain.Entities;
using Tarifa.Domain.Interfaces;

namespace Tarifa.Infrastructure.Data
{
    public class TarifacaoRepository : ITarifacaoRepository
    {
        private readonly string _connectionString;

        public TarifacaoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        private IDbConnection CreateConnection() => new SqliteConnection(_connectionString);

        public async Task AdicionarAsync(Tarifacao tarifacao)
        {
            using var db = CreateConnection();
            var sql = @"INSERT INTO tarifacao
                        (numerocontacorrente, idrequisicaotransferencia, valor, datatarfacao)
                        VALUES (@NumeroContaCorrente, @IdRequisicaoTransferencia, @Valor, @DataTarifacao);
                        SELECT last_insert_rowid();";

            tarifacao.IdTarifacao = await db.ExecuteScalarAsync<int>(sql, tarifacao);
        }

        public async Task<Tarifacao?> ObterPorIdRequisicaoAsync(string idRequisicaoTransferencia)
        {
            using var db = CreateConnection();
            var sql = @"SELECT idtarifacao AS IdTarifacao,
                               numerocontacorrente AS NumeroContaCorrente,
                               idrequisicaotransferencia AS IdRequisicaoTransferencia,
                               valor AS Valor,
                               datatarfacao AS DataTarifacao
                        FROM tarifacao
                        WHERE idrequisicaotransferencia = @IdRequisicaoTransferencia";

            return await db.QueryFirstOrDefaultAsync<Tarifacao>(sql, new { IdRequisicaoTransferencia = idRequisicaoTransferencia });
        }
    }
}
