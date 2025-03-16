using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testcontainers.MsSql;

namespace InventoryManagement.Tests.Integration
{
    public class TestDatabaseFixture : IAsyncLifetime
    {
        private readonly MsSqlContainer _dbContainer;

        public string ConnectionString => _dbContainer.GetConnectionString();

        public TestDatabaseFixture()
        {
            _dbContainer = new MsSqlBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2022-latest") // Версия SQL Server
                .WithPassword("InventoryManagement") // Пароль должен соответствовать требованиям SQL Server
                .Build();
        }

        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await _dbContainer.StopAsync();
        }
    }  
}
