using FluentAssertions;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Application.Services;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Interfaces;
using Moq;

namespace InventoryManagement.Tests.UnitTests.Services
{
    public class LocationServiceTests
    {
        private readonly Mock<ILocationRepository> _locationRepositoryMock;
        private readonly ILocationService _locationService;

        public LocationServiceTests()
        {
            _locationRepositoryMock = new Mock<ILocationRepository>();
            _locationService = new LocationService(_locationRepositoryMock.Object);
        }

        [Fact]
        public async Task GetAllLocations_ShouldReturnListOfLocations()
        {
            // Arrange
            var locations = new List<Location>
            {
                new Location { Id = 1, Name = "Warehouse A", Address = "123 Street" },
                new Location { Id = 2, Name = "Warehouse B", Address = "456 Avenue" }
            };

            _locationRepositoryMock.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(locations);

            // Act
            var result = await _locationService.GetAllLocations();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(locations);
        }

        [Fact]
        public async Task GetLocationById_ShouldReturnLocation_WhenLocationExists()
        {
            // Arrange
            var locationId = 1;
            var expectedLocation = new Location 
            { 
                Id = locationId,
                Name = "Main Storage",
                Address = "789 Road" 
            };

            _locationRepositoryMock.Setup(repo => repo.GetByIdAsync(locationId))
                .ReturnsAsync(expectedLocation);

            // Act
            var result = await _locationService.GetLocationById(locationId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedLocation);
        }

        [Fact]
        public async Task GetLocationById_ShouldReturnNull_WhenLocationDoesNotExist()
        {
            // Arrange
            _locationRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Location)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _locationService.GetLocationById(999));
        }

        [Fact]
        public async Task AddLocation_ShouldCallRepositoryOnce()
        {
            // Arrange
            var newLocation = new Location { Id = 3, Name = "New Warehouse", Address = "1011 Blvd" };

            // Act
            await _locationService.AddLocation(newLocation);

            // Assert
            _locationRepositoryMock.Verify(repo => repo.AddAsync(newLocation), Times.Once);
        }

        [Fact]
        public async Task UpdateLocation_ShouldCallRepositoryOnce()
        {
            // Arrange
            var updatedLocation = new Location { Id = 4, Name = "Updated Storage", Address = "1213 Street" };

            // Act
            await _locationService.UpdateLocation(updatedLocation);

            // Assert
            _locationRepositoryMock.Verify(repo => repo.UpdateAsync(updatedLocation), Times.Once);
        }

        [Fact]
        public async Task DeleteLocation_ShouldCallRepositoryOnce()
        {
            // Arrange
            var locationId = 5;

            // Act
            await _locationService.DeleteLocation(locationId);

            // Assert
            _locationRepositoryMock.Verify(repo => repo.DeleteAsync(locationId), Times.Once);
        }
    }
}
