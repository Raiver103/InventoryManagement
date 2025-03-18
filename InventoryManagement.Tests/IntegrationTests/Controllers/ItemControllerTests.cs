using InventoryManagement.Domain.Entities;
using InventoryManagement.WEB;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Net;
using InventoryManagement.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;

namespace InventoryManagement.Tests.IntegrationTests.Controllers
{
    [Collection("Sequential")]
    public class ItemControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;
        private readonly string _connectionString = "Data Source=RAIVER\\MSSQLSERVER103;Initial Catalog=InventoryManagement.Tests;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

        public ItemControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Удаляем старый контекст, если он уже зарегистрирован
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Добавляем новый SQL Server контекст
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlServer(_connectionString));

                    // Создаем новый scope и заполняем БД тестовыми данными
                    var sp = services.BuildServiceProvider();
                    using (var scope = sp.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        context.Database.EnsureDeleted();  // Удаляем БД перед тестами
                        context.Database.EnsureCreated();  // Создаем новую
                        SeedTestData(context);
                    }
                });
            });

            _client = _factory.CreateClient();
        }

        private void SeedTestData(AppDbContext context)
        {
            context.Database.EnsureDeleted(); // Удаляем старую БД
            context.Database.EnsureCreated(); // Создаем новую

            // ✅ Создаем Locations БЕЗ указания Id (SQL Server сам присвоит значения)
            var locations = new List<Location>
    {
        new Location { Name = "Warehouse A", Address = "123 Main St" },
        new Location { Name = "Warehouse B", Address = "456 Side St" }
    };

            context.Locations.AddRange(locations);
            context.SaveChanges();

            // ✅ Получаем ID, которые были сгенерированы
            var locationA = context.Locations.FirstOrDefault(l => l.Name == "Warehouse A");
            var locationB = context.Locations.FirstOrDefault(l => l.Name == "Warehouse B");

            // ✅ Теперь создаем Items, указывая корректный LocationId
            var items = new List<Item>
    {
        new Item { Name = "Test Item 1", Quantity = 10, Category = "Test Category 1", LocationId = locationA.Id },
        new Item { Name = "Test Item 2", Quantity = 20, Category = "Test Category 2", LocationId = locationB.Id }
    };

            context.Items.AddRange(items);
            context.SaveChanges();
        }


        [Fact]
        public async Task GetAllItems_ShouldReturnAllItems()
        {
            var response = await _client.GetAsync("/api/item");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var items = JsonConvert.DeserializeObject<IEnumerable<Item>>(content);
            Assert.Equal(2, items.Count());
        }

        [Fact]
        public async Task GetItemById_ShouldReturnItem_WhenItemExists()
        {
            var response = await _client.GetAsync("/api/item/1");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var item = JsonConvert.DeserializeObject<Item>(content);
            Assert.NotNull(item);
            Assert.Equal(1, item.Id);
        }

        [Fact]
        public async Task CreateItem_ShouldReturnCreatedItem()
        {
            var newItem = new
            {
                Name = "New Item",
                Quantity = 5,
                Category = "Test Category",
                LocationId = 1  // Добавляем LocationId
            };

            var response = await _client.PostAsJsonAsync("/api/item", newItem);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var createdItem = JsonConvert.DeserializeObject<Item>(content);

            Assert.NotNull(createdItem);
            Assert.Equal("New Item", createdItem.Name);
        }

        [Fact]
        public async Task UpdateItem_ShouldReturnNoContent()
        {
            var updatedItem = new { Name = "Updated Item", Quantity = 99, Category = "Updated Category", LocationId = 1 };
            var response = await _client.PutAsJsonAsync("/api/item/1", updatedItem);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteItem_ShouldReturnNoContent()
        {
            var response = await _client.DeleteAsync("/api/item/1");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}
