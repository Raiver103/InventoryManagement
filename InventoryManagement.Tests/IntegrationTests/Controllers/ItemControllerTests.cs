//using InventoryManagement.Domain.Entities;
//using InventoryManagement.WEB;
//using Microsoft.AspNetCore.Mvc.Testing;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.DependencyInjection;
//using Newtonsoft.Json;
//using System.Net.Http.Json;
//using System.Net;
//using InventoryManagement.Infrastructure.Persistence;

//namespace InventoryManagement.Tests.IntegrationTests.Controllers
//{
//    [Collection("Sequential")]
//    public class ItemControllerTests : IClassFixture<WebApplicationFactory<Program>>, IClassFixture<SqlServerFixture>
//    {
//        private readonly HttpClient _client;

//        public ItemControllerTests(WebApplicationFactory<Program> factory, SqlServerFixture sqlFixture)
//        {
//            var connectionString = sqlFixture.ConnectionString;

//            var _factory = factory.WithWebHostBuilder(builder =>
//            {
//                builder.ConfigureServices(services =>
//                {
//                    var descriptor = services.SingleOrDefault(
//                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

//                    if (descriptor != null)
//                    {
//                        services.Remove(descriptor);
//                    }

//                    services.AddDbContext<AppDbContext>(options =>
//                        options.UseSqlServer(connectionString));

//                    var sp = services.BuildServiceProvider();
//                    using (var scope = sp.CreateScope())
//                    {
//                        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//                        context.Database.Migrate(); // ✅ Вместо EnsureDeleted() + EnsureCreated()
//                        SeedTestData(context);
//                    }
//                });
//            });

//            _client = _factory.CreateClient();
//        }

//        private void SeedTestData(AppDbContext context)
//        {
//            context.Items.RemoveRange(context.Items);
//            context.Locations.RemoveRange(context.Locations);
//            context.SaveChanges();

//            // ✅ 1. Добавляем тестовые локации (убираем Id)
//            var locations = new List<Location>
//            {
//                new Location { Name = "Warehouse A", Address = "ывыв" },
//                new Location { Name = "Warehouse B", Address = "ывыв" }
//            };
//            context.Locations.AddRange(locations);
//            context.SaveChanges();

//            // ✅ 2. Получаем реальные `Id`, сгенерированные БД
//            int locationId1 = locations[0].Id;
//            int locationId2 = locations[1].Id;

//            // ✅ 3. Добавляем тестовые товары, указывая `LocationId`
//            context.Items.AddRange(
//                new Item { Name = "Test Item 1", Quantity = 10, Category = "Test Category 1", LocationId = locationId1 },
//                new Item { Name = "Test Item 2", Quantity = 20, Category = "Test Category 2", LocationId = locationId2 }
//            );
//            context.SaveChanges();
//        }


//        [Fact]
//        public async Task GetAllItems_ShouldReturnAllItems()
//        {
//            var response = await _client.GetAsync("/api/item");
//            response.EnsureSuccessStatusCode();
//            var content = await response.Content.ReadAsStringAsync();
//            var items = JsonConvert.DeserializeObject<IEnumerable<Item>>(content);
//            Assert.Equal(2, items.Count());
//        }

//        [Fact]
//        public async Task GetItemById_ShouldReturnItem_WhenItemExists()
//        {
//            var response = await _client.GetAsync("/api/item/1");
//            response.EnsureSuccessStatusCode();
//            var content = await response.Content.ReadAsStringAsync();
//            var item = JsonConvert.DeserializeObject<Item>(content);
//            Assert.NotNull(item);
//            Assert.Equal(1, item.Id);
//        }

//        [Fact]
//        public async Task CreateItem_ShouldReturnCreatedItem()
//        {
//            var newItem = new { Name = "New Item", Quantity = 5, Category = "Test Category" };
//            var response = await _client.PostAsJsonAsync("/api/item", newItem);
//            response.EnsureSuccessStatusCode();
//            var content = await response.Content.ReadAsStringAsync();
//            var createdItem = JsonConvert.DeserializeObject<Item>(content);
//            Assert.NotNull(createdItem);
//            Assert.Equal("New Item", createdItem.Name);
//        }

//        [Fact]
//        public async Task UpdateItem_ShouldReturnNoContent()
//        {
//            var updatedItem = new { Name = "Updated Item", Quantity = 99, Category = "Updated Category" };
//            var response = await _client.PutAsJsonAsync("/api/item/1", updatedItem);
//            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
//        }

//        [Fact]
//        public async Task DeleteItem_ShouldReturnNoContent()
//        {
//            var response = await _client.DeleteAsync("/api/item/1");
//            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
//        }
//    }
//}
