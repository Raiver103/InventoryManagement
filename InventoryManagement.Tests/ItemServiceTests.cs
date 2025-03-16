using FluentAssertions;
using InventoryManagement.Application.Services;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Tests
{
    public class ItemServiceTests
    {
        private readonly Mock<IItemRepository> _itemRepositoryMock;
        private readonly ItemService _itemService;

        public ItemServiceTests()
        {
            _itemRepositoryMock = new Mock<IItemRepository>();
            _itemService = new ItemService(_itemRepositoryMock.Object);
        }

        [Fact]
        public async Task GetAllItems_ShouldReturnListOfItems()
        {
            // Arrange
            var items = new List<Item>
        {
            new Item { Id = 1, Name = "Laptop", Quantity = 5 },
            new Item { Id = 2, Name = "Mouse", Quantity = 10 }
        };

            _itemRepositoryMock.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(items);

            // Act
            var result = await _itemService.GetAllItems();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(items);
        }

        [Fact]
        public async Task GetItemById_ShouldReturnItem_WhenItemExists()
        {
            // Arrange
            var itemId = 1;
            var expectedItem = new Item { Id = itemId, Name = "Keyboard", Quantity = 3 };

            _itemRepositoryMock.Setup(repo => repo.GetByIdAsync(itemId))
                .ReturnsAsync(expectedItem);

            // Act
            var result = await _itemService.GetItemById(itemId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedItem);
        }

        [Fact]
        public async Task GetItemById_ShouldReturnNull_WhenItemDoesNotExist()
        {
            // Arrange
            _itemRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Item)null);

            // Act
            var result = await _itemService.GetItemById(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AddItem_ShouldCallRepositoryOnce()
        {
            // Arrange
            var newItem = new Item { Id = 3, Name = "Monitor", Quantity = 2 };

            // Act
            await _itemService.AddItem(newItem);

            // Assert
            _itemRepositoryMock.Verify(repo => repo.AddAsync(newItem), Times.Once);
        }

        [Fact]
        public async Task UpdateItem_ShouldCallRepositoryOnce()
        {
            // Arrange
            var updatedItem = new Item { Id = 4, Name = "Desk", Quantity = 1 };

            // Act
            await _itemService.UpdateItem(updatedItem);

            // Assert
            _itemRepositoryMock.Verify(repo => repo.UpdateAsync(updatedItem), Times.Once);
        }

        [Fact]
        public async Task DeleteItem_ShouldCallRepositoryOnce()
        {
            // Arrange
            var itemId = 5;

            // Act
            await _itemService.DeleteItem(itemId);

            // Assert
            _itemRepositoryMock.Verify(repo => repo.DeleteAsync(itemId), Times.Once);
        }
    }
}
