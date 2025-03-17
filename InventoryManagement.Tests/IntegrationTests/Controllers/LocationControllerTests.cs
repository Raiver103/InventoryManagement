using InventoryManagement.Application.DTOs.Location;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Infastructure.Persistence;
using InventoryManagement.WEB;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace InventoryManagement.Tests.IntegrationTests.Controllers
{
    [Collection("Sequential")]
    public class LocationControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;

        public LocationControllerTests(WebApplicationFactory<Program> factory)
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
            context.Locations.AddRange(
                new Location { Id = 1, Name = "Warehouse A", Address = "123 Main St" },
                new Location { Id = 2, Name = "Warehouse B", Address = "456 Side St" },
                new Location { Id = 3, Name = "Warehouse C", Address = "666 Side St" }
            );
            context.SaveChanges();
        }

        [Fact]
        public async Task GetAllLocations_ShouldReturnAllLocations()
        {
            // Act
            var response = await _client.GetAsync("/api/location");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var locations = JsonConvert.DeserializeObject<IEnumerable<Location>>(content);

            Assert.Equal(3, locations.Count());
        }

        [Fact]
        public async Task GetLocationById_ShouldReturnLocation_WhenExists()
        {
            // Act
            var response = await _client.GetAsync("/api/location/1");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var location = JsonConvert.DeserializeObject<Location>(content);

            Assert.NotNull(location);
            Assert.Equal(1, location.Id);
        }

        [Fact]
        public async Task GetLocationById_ShouldReturnNotFound_WhenNotExists()
        {
            // Act
            var response = await _client.GetAsync("/api/location/999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateLocation_ShouldReturnCreatedLocation()
        {
            // Arrange
            var newLocation = new { Name = "New Warehouse", Address = "789 Another St" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/location", newLocation);

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var createdLocation = JsonConvert.DeserializeObject<Location>(content);

            Assert.NotNull(createdLocation);
            Assert.Equal("New Warehouse", createdLocation.Name);
        }

        [Fact]
        public async Task UpdateLocation_ShouldReturnNoContent()
        {
            // Arrange
            var updatedLocation = new { Name = "Updated Warehouse", Address = "Updated Address" };

            // Act
            var response = await _client.PutAsJsonAsync("/api/location/1", updatedLocation);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task UpdateLocation_ShouldReturnNotFound_WhenLocationDoesNotExist()
        {
            // Arrange
            var updatedLocation = new { Name = "Nonexistent Warehouse", Address = "Nowhere" };

            // Act
            var response = await _client.PutAsJsonAsync("/api/locations/999", updatedLocation);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteLocation_ShouldReturnNoContent_WhenExists()
        {
            // Act
            var response = await _client.DeleteAsync("/api/location/1");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteLocation_ShouldReturnNotFound_WhenNotExists()
        {
            // Act
            var response = await _client.DeleteAsync("/api/locations/999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
