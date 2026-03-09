using Autenticacao.Domain.Entities;
using Autenticacao.Domain.Interfaces;
using Dapper;
using Npgsql;
using System.Data;

namespace Autenticacao.Infrastructure.Data
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly string _connectionString;

        public UsuarioRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

        public async Task<Usuario?> ObterPorCpfAsync(string cpf)
        {
            using var db = CreateConnection();
            var sql = "SELECT id, cpf, nome, senhahash AS SenhaHash, numeroconta AS NumeroConta, ativo, datacriacao AS DataCriacao FROM usuario WHERE cpf = @Cpf";
            return await db.QueryFirstOrDefaultAsync<Usuario>(sql, new { Cpf = cpf });
        }

        public async Task<Usuario?> ObterPorNumeroContaAsync(string numeroConta)
        {
            using var db = CreateConnection();
            var sql = "SELECT id, cpf, nome, senhahash AS SenhaHash, numeroconta AS NumeroConta, ativo, datacriacao AS DataCriacao FROM usuario WHERE numeroconta = @NumeroConta";
            return await db.QueryFirstOrDefaultAsync<Usuario>(sql, new { NumeroConta = numeroConta });
        }

        public async Task<int> AdicionarAsync(Usuario usuario)
        {
            using var db = CreateConnection();
            var sql = @"INSERT INTO usuario (cpf, nome, senhahash, numeroconta, ativo, datacriacao)
                        VALUES (@Cpf, @Nome, @SenhaHash, @NumeroConta, @Ativo, @DataCriacao)
                        RETURNING id;";
            return await db.QuerySingleAsync<int>(sql, usuario);
        }

        public async Task InativarAsync(string numeroConta)
        {
            using var db = CreateConnection();
            var sql = "UPDATE usuario SET ativo = 0 WHERE numeroconta = @NumeroConta";
            await db.ExecuteAsync(sql, new { NumeroConta = numeroConta });
        }
    }
}
