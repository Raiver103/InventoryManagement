using InventoryManagement.Domain.Entities;
using InventoryManagement.Infrastructure.Persistence;
using InventoryManagement.WEB;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Net;
using Newtonsoft.Json;

namespace InventoryManagement.Tests.IntegrationTests.Controllers
{
    [Collection("Sequential")]
    public class TransactionControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;
        //private readonly string _connectionString = "Server=inventory_db_tests,1433;Database=InventoryManagement.Tests;User Id=sa;Password=Strong!Password@123;TrustServerCertificate=True;";
        private readonly string _connectionString = "Server=localhost,1434;Database=InventoryManagement.Tests;User Id=sa;Password=Strong!Password@123;TrustServerCertificate=True;";
        
        public TransactionControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlServer(_connectionString));

                    var sp = services.BuildServiceProvider();
                    using (var scope = sp.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        context.Database.EnsureDeleted();
                        context.Database.EnsureCreated();
                        SeedTestData(context);
                    }
                });
            });
            _client = _factory.CreateClient();
        }

        private void SeedTestData(AppDbContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var locations = new List<Location>
    {
        new Location { Name = "Warehouse A", Address = "Address 1" },
        new Location { Name = "Warehouse B", Address = "Address 2" },
        new Location { Name = "Warehouse C", Address = "Address 3" }
    };

            context.Locations.AddRange(locations);
            context.SaveChanges(); // Сначала сохраняем, чтобы у локаций были сгенерированные ID

            var item = new Item
            {
                Name = "Test Item",
                Quantity = 10,
                Category = "Test Category",
                LocationId = locations[0].Id // Используем реальный ID из базы
            };
            context.Items.Add(item);
            context.SaveChanges();

            var user = new User
            {
                Id = "1",
                FirstName = "Test FirstName",
                LastName = "Test LastName",
                Email = "Test Email",
                Role = "Test Role",
                PasswordHash = "TestPasswordHash"
            };
            context.Users.Add(user);
            context.SaveChanges();

            var transactions = new List<Transaction>
    {
        new Transaction { ItemId = item.Id, FromLocationId = locations[0].Id, ToLocationId = locations[1].Id, UserId = user.Id },
        new Transaction { ItemId = item.Id, FromLocationId = locations[1].Id, ToLocationId = locations[2].Id, UserId = user.Id }
    };

            context.Transactions.AddRange(transactions);
            context.SaveChanges();
        }


        [Fact]
        public async Task GetAllTransactions_ShouldReturnAllTransactions()
        {
            var response = await _client.GetAsync("/api/transaction");
            response.EnsureSuccessStatusCode();
            var transactions = JsonConvert.DeserializeObject<IEnumerable<Transaction>>(await response.Content.ReadAsStringAsync());
            Assert.Equal(2, transactions.Count());
        }

        [Fact]
        public async Task GetTransactionById_ShouldReturnTransaction_WhenTransactionExists()
        {
            var response = await _client.GetAsync("/api/transaction/1");
            response.EnsureSuccessStatusCode();
            var transaction = JsonConvert.DeserializeObject<Transaction>(await response.Content.ReadAsStringAsync());
            Assert.NotNull(transaction);
            Assert.Equal(1, transaction.Id);
        }
    }
}
