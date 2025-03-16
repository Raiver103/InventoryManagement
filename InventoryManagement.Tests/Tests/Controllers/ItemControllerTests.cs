using InventoryManagement.Domain.Entities;
using InventoryManagement.Infastructure.Persistence;
using InventoryManagement.WEB;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Net;

namespace InventoryManagement.Tests.Tests.Controllers
{
    public class ItemControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;

        public ItemControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Удаляем существующую конфигурацию контекста
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Добавляем новый InMemoryDatabase
                    services.AddDbContext<AppDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("InMemoryDbForTesting");
                    });

                    // Создаем новый scope и заполняем базу тестовыми данными
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
            context.Database.EnsureDeleted(); // Удаляем предыдущую БД, если есть
            context.Database.EnsureCreated();
            context.Items.AddRange(
                new Item { Id = 1, Name = "Test Item 1", Quantity = 10, Category = "Test Category 1" },
                new Item { Id = 2, Name = "Test Item 2", Quantity = 20, Category = "Test Category 2" }
            );
            context.SaveChanges();
        }

        [Fact]
        public async Task GetAllItems_ShouldReturnAllItems()
        {
            // Act
            var response = await _client.GetAsync("/api/item");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var items = JsonConvert.DeserializeObject<IEnumerable<Item>>(content);
            Assert.Equal(2, items.Count());
        }

        [Fact]
        public async Task GetItemById_ShouldReturnItem_WhenItemExists()
        {
            // Act
            var response = await _client.GetAsync("/api/item/1");

            // Проверяем статус ответа
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            // Логирование ответа для отладки
            Console.WriteLine($"Response content: {content}");

            // Десериализация JSON
            var item = JsonConvert.DeserializeObject<Item>(content);

            // Assert
            Assert.NotNull(item);
            Assert.Equal(1, item.Id);
        }
        [Fact]
        public async Task CreateItem_ShouldReturnCreatedItem()
        {
            // Arrange
            var newItem = new { Name = "New Item", Quantity = 5, Category = "Test Category" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/item", newItem);

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var createdItem = JsonConvert.DeserializeObject<Item>(content);

            Assert.NotNull(createdItem);
            Assert.Equal("New Item", createdItem.Name);
        }

        [Fact]
        public async Task UpdateItem_ShouldReturnNoContent()
        {
            // Arrange
            var updatedItem = new { Name = "Updated Item", Quantity = 99, Category = "Updated Category" };

            // Act
            var response = await _client.PutAsJsonAsync("/api/item/1", updatedItem);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteItem_ShouldReturnNoContent()
        {
            // Act
            var response = await _client.DeleteAsync("/api/item/1");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

    }
}
