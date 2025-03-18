using Microsoft.Data.SqlClient;

public class SqlServerFixture : IDisposable
{
    public string ConnectionString { get; }

    public SqlServerFixture()
    {
        ConnectionString = "Data Source=RAIVER\\MSSQLSERVER103;Initial Catalog=InventoryManagement.Tests;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

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
