using DbUp;
using System.Reflection;

namespace Transferencia.Infrastructure.Data
{
    public static class DatabaseBootstrap
    {
        public static void Setup(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var upgrader = DeployChanges.To
                .SqliteDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .LogToConsole()
                .Build();

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                throw new Exception("Falha ao rodar as migrações do banco de dados: " + result.Error);
            }
        }
    }
}
