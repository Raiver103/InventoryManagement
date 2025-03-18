﻿using InventoryManagement.Application.DTOs.Location;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Infrastructure.Persistence;
using InventoryManagement.WEB;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Json;

namespace InventoryManagement.Tests.IntegrationTests.Controllers
{
    [Collection("Sequential")]
    public class LocationControllerTests : IClassFixture<WebApplicationFactory<Program>>, IClassFixture<SqlServerFixture>
    {
        private readonly HttpClient _client;

        public LocationControllerTests(WebApplicationFactory<Program> factory, SqlServerFixture sqlFixture)
        {
            var connectionString = sqlFixture.ConnectionString;

            var _factory = factory.WithWebHostBuilder(builder =>
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
                        options.UseSqlServer(connectionString));

                    var sp = services.BuildServiceProvider();
                    using (var scope = sp.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        context.Database.Migrate();
                        SeedTestData(context);
                    }
                });
            });

            _client = _factory.CreateClient();
        }

        private void SeedTestData(AppDbContext context)
        {
            context.Locations.RemoveRange(context.Locations);
            context.SaveChanges();

            var locations = new List<Location>
            {
                new Location { Name = "Warehouse A", Address = "123 Main St" },
                new Location { Name = "Warehouse B", Address = "456 Side St" },
                new Location { Name = "Warehouse C", Address = "666 Side St" }
            };

            context.Locations.AddRange(locations);
            context.SaveChanges();
        }

        [Fact]
        public async Task GetAllLocations_ShouldReturnAllLocations()
        {
            var response = await _client.GetAsync("/api/location");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var locations = JsonConvert.DeserializeObject<IEnumerable<Location>>(content);
            Assert.Equal(3, locations.Count());
        }

        [Fact]
        public async Task GetLocationById_ShouldReturnLocation_WhenExists()
        {
            // Отправляем запрос на получение локации с ID = 1
            var response = await _client.GetAsync("/api/locations/1");

            // Проверяем, что ответ имеет статус 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Читаем тело ответа
            var content = await response.Content.ReadAsStringAsync();

            // Десериализуем в DTO
            var location = JsonConvert.DeserializeObject<LocationResponseDTO>(content);

            // Проверяем, что объект не null и у него правильный ID
            Assert.NotNull(location);
            Assert.Equal(1, location.Id);
        }

        [Fact]
        public async Task GetLocationById_ShouldReturnNoContent_WhenNotExists()
        {
            var response = await _client.GetAsync("/api/location/999");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
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
