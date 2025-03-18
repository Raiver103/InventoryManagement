using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testcontainers.MsSql;

namespace InventoryManagement.Tests
{
    public class SqlServerFixture : IAsyncLifetime
    {
        public MsSqlContainer DbContainer { get; } = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("Strong!Password@123")
            .Build();

        public string ConnectionString => DbContainer.GetConnectionString();

        public async Task InitializeAsync()
        {
            await DbContainer.StartAsync();

            var masterConnectionString = $"Server=localhost,{DbContainer.GetMappedPublicPort(1433)};Database=master;User Id=sa;Password=Strong!Password@123;TrustServerCertificate=True;";

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
