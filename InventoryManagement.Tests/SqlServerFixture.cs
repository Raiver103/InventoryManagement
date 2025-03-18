using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Testcontainers.MsSql;

namespace InventoryManagement.Tests
{
    public class SqlServerFixture : IAsyncLifetime
    {
        public MsSqlContainer DbContainer { get; } = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("Strong!Password@123")
            .WithEnvironment("ACCEPT_EULA", "Y") // ✅ Флаг для запуска в CI/CD
            .Build();

        public string ConnectionString => $"{DbContainer.GetConnectionString()};Database=TestInventoryManagement"; // ✅ Добавляем БД

        public async Task InitializeAsync()
        {
            await DbContainer.StartAsync();

            var masterConnectionString = $"{DbContainer.GetConnectionString()};Database=master";

            using var connection = new SqlConnection(masterConnectionString);
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'TestInventoryManagement')
                BEGIN
                    CREATE DATABASE TestInventoryManagement;
                END";
            await command.ExecuteNonQueryAsync();
        }

        public async Task DisposeAsync()
        {
            await DbContainer.DisposeAsync();
        }
    }
}
