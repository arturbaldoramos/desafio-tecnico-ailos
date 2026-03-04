using DbUp;
using DbUp.Sqlite;
using System.Reflection;

namespace BankMore.Infrastructure.Data;

public static class DatabaseBootstrap
{
    public static void Setup(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var upgrader = DeployChanges.To
            .SqliteDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly()) // Procura os scripts SQL embutidos
            .LogToConsole()
            .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            throw new Exception("Falha ao rodar as migrações do banco de dados: " + result.Error);
        }
    }
}