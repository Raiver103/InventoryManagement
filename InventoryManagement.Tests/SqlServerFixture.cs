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
        }

        public async Task DisposeAsync()
        {
            await DbContainer.DisposeAsync();
        }
    }
}
