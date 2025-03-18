using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Testcontainers.MsSql;

namespace InventoryManagement.Tests
{
    public class SqlServerFixture : IDisposable
    {
        public string ConnectionString { get; }

        public SqlServerFixture()
        {
            ConnectionString = "Server=localhost,1433;Database=TestDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True;";
            EnsureDatabaseCreated();
        }

        private void EnsureDatabaseCreated()
        {
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();
            using var command = new SqlCommand("IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TestDb') CREATE DATABASE TestDb;", connection);
            command.ExecuteNonQuery();
        }

        public void Dispose()
        {
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();
            using var command = new SqlCommand("DROP DATABASE IF EXISTS TestDb;", connection);
            command.ExecuteNonQuery();
        }
    }

}
