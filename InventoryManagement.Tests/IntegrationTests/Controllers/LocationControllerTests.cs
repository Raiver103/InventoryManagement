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
    public class LocationControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;
        //private readonly string _connectionString = "Server=inventory_db_tests,1433;Database=InventoryManagement.Tests;User Id=sa;Password=Strong!Password@123;TrustServerCertificate=True;";
        private readonly string _connectionString = "Server=localhost,1434;Database=InventoryManagement.Tests;User Id=sa;Password=Strong!Password@123;TrustServerCertificate=True;";

        public LocationControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
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
            WaitForSqlServer();

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var locations = new List<Location>
            {
                new Location { Name = "Warehouse A", Address = "123 Main St" },
                new Location { Name = "Warehouse B", Address = "456 Side St" }
            };

            context.Locations.AddRange(locations);
            context.SaveChanges();
        }

        private void WaitForSqlServer()
        {
            using var connection = new SqlConnection(_connectionString);
            for (int i = 0; i < 10; i++)  // Даем 10 попыток
            {
                try
                {
                    connection.Open();
                    return;  // Если успешно — выходим
                }
                catch
                {
                    Thread.Sleep(5000);  // Ждем 5 секунд перед повтором
                }
            }
            throw new Exception("Не удалось подключиться к SQL Server в Docker");
        }


        [Fact]
        public async Task GetAllLocations_ShouldReturnAllLocations()
        {
            var response = await _client.GetAsync("/api/location");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var locations = JsonConvert.DeserializeObject<IEnumerable<Location>>(content);
            Assert.Equal(2, locations.Count());
        }

        [Fact]
        public async Task GetLocationById_ShouldReturnLocation_WhenExists()
        {
            var response = await _client.GetAsync("/api/location/1");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var location = JsonConvert.DeserializeObject<Location>(content);
            Assert.NotNull(location);
            Assert.Equal(1, location.Id);
        }

        [Fact]
        public async Task GetLocationById_ShouldReturnNotFound_WhenNotExists()
        {
            var response = await _client.GetAsync("/api/location/999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateLocation_ShouldReturnCreatedLocation()
        {
            var newLocation = new { Name = "New Warehouse", Address = "789 Another St" };
            var response = await _client.PostAsJsonAsync("/api/location", newLocation);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var createdLocation = JsonConvert.DeserializeObject<Location>(content);
            Assert.NotNull(createdLocation);
            Assert.Equal("New Warehouse", createdLocation.Name);
        }

        [Fact]
        public async Task UpdateLocation_ShouldReturnNoContent()
        {
            var updatedLocation = new { Name = "Updated Warehouse", Address = "Updated Address" };
            var response = await _client.PutAsJsonAsync("/api/location/1", updatedLocation);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task UpdateLocation_ShouldReturnNotFound_WhenLocationDoesNotExist()
        {
            var updatedLocation = new { Name = "Nonexistent Warehouse", Address = "Nowhere" };
            var response = await _client.PutAsJsonAsync("/api/location/999", updatedLocation);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteLocation_ShouldReturnNoContent_WhenExists()
        {
            var response = await _client.DeleteAsync("/api/location/1");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteLocation_ShouldReturnNotFound_WhenNotExists()
        {
            var response = await _client.DeleteAsync("/api/location/999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
