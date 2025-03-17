using InventoryManagement.Domain.Entities;
using InventoryManagement.Infastructure.Persistence;
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
                    {
                        options.UseInMemoryDatabase("InMemoryDbForTesting");
                    });

                    var sp = services.BuildServiceProvider();
                    using (var scope = sp.CreateScope())
                    {
                        var scopedServices = scope.ServiceProvider;
                        var context = scopedServices.GetRequiredService<AppDbContext>();
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

            var item = new Item { Id = 1, Name = "Test Item", Quantity = 10, Category = "Test Category", LocationId = 1 };
            context.Items.Add(item);
            var user = new User
            { Id = "1", FirstName = "Test FirstName", LastName = "Test LastName", Email = "Test Email", Role = "Test Role", PasswordHash = "TestPasswordHash" };
            context.Users.Add(user);
            context.Locations.AddRange(
                new Location { Id = 1, Name = "Warehouse A", Address = "Address 1" },
                new Location { Id = 2, Name = "Warehouse B", Address = "Address 2" }
             );
            context.Transactions.AddRange(
                new Transaction { Id = 1, ItemId = 1, FromLocationId = 1, ToLocationId = 2, UserId = "1" },
                new Transaction { Id = 2, ItemId = 1, FromLocationId = 2, ToLocationId = 3, UserId = "1" }
            );
            context.SaveChanges();
        }

        [Fact]
        public async Task GetAllTransactions_ShouldReturnAllTransactions()
        {
            var response = await _client.GetAsync("/api/transaction");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var transactions = JsonConvert.DeserializeObject<IEnumerable<Transaction>>(content);
            Assert.Equal(2, transactions.Count());
        }

        [Fact]
        public async Task GetTransactionById_ShouldReturnTransaction_WhenTransactionExists()
        {
            var response = await _client.GetAsync("/api/transaction/1");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var transaction = JsonConvert.DeserializeObject<Transaction>(content);
            Assert.NotNull(transaction);
            Assert.Equal(1, transaction.Id);
        }

        [Fact]
        public async Task CreateTransaction_ShouldReturnCreatedTransaction()
        {
            var newTransaction = new Transaction { ItemId = 1, FromLocationId = 1, ToLocationId = 2, UserId = "1", Id = 1, Timestamp = DateTime.Now };
            var response = await _client.PostAsJsonAsync("/api/transaction", newTransaction);

            var content = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
            var createdTransaction = JsonConvert.DeserializeObject<Transaction>(content);
            Assert.NotNull(createdTransaction);
            Assert.Equal(1, createdTransaction.ItemId);
        }

        [Fact]
        public async Task CreateTransaction_ShouldReturnBadRequest_WhenMovingToSameLocation()
        {
            var invalidTransaction = new { ItemId = 1, FromLocationId = 1, ToLocationId = 1 };
            var response = await _client.PostAsJsonAsync("/api/transaction", invalidTransaction);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
