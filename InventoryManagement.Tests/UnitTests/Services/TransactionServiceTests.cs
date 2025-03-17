using FluentAssertions;
using InventoryManagement.Application.DTOs.Transaction;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Application.Services;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Interfaces;
using Moq;

namespace InventoryManagement.Tests.UnitTests.Services
{
    public class TransactionServiceTests
    {
        private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
        private readonly Mock<IItemRepository> _itemRepositoryMock;
        private readonly Mock<ILocationRepository> _locationRepositoryMock;
        private readonly ITransactionService _transactionService;

        public TransactionServiceTests()
        {
            _transactionRepositoryMock = new Mock<ITransactionRepository>();
            _itemRepositoryMock = new Mock<IItemRepository>();
            _locationRepositoryMock = new Mock<ILocationRepository>();

            _transactionService = new TransactionService(
                _transactionRepositoryMock.Object,
                _itemRepositoryMock.Object,
                _locationRepositoryMock.Object);
        }

        [Fact]
        public async Task GetAllTransactions_ShouldReturnListOfTransactions()
        {
            // Arrange
            var transactions = new List<Transaction>
            {
                new Transaction { Id = 1, ItemId = 10, FromLocationId = 1, ToLocationId = 2, UserId = "user1" },
                new Transaction { Id = 2, ItemId = 11, FromLocationId = 2, ToLocationId = 3, UserId = "user2" }
            };

            _transactionRepositoryMock.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(transactions);

            // Act
            var result = await _transactionService.GetAllTransactions();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(transactions);
        }

        [Fact]
        public async Task GetTransactionById_ShouldReturnTransaction_WhenTransactionExists()
        {
            // Arrange
            var transactionId = 1;
            var expectedTransaction = new Transaction { Id = transactionId, ItemId = 10, FromLocationId = 1, ToLocationId = 2, UserId = "user1" };

            _transactionRepositoryMock.Setup(repo => repo.GetByIdAsync(transactionId))
                .ReturnsAsync(expectedTransaction);

            // Act
            var result = await _transactionService.GetTransactionById(transactionId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedTransaction);
        }

        [Fact]
        public async Task GetTransactionById_ShouldReturnNull_WhenTransactionDoesNotExist()
        {
            // Arrange
            _transactionRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Transaction)null);

            // Act
            var result = await _transactionService.GetTransactionById(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AddTransaction_ShouldUpdateItemLocation()
        {
            // Arrange
            var transactionCreateDto = new TransactionCreateDTO
            {
                ItemId = 10,
                FromLocationId = 1,
                ToLocationId = 2,
                UserId = "user1",
                Timestamp = DateTime.UtcNow
            };

            var item = new Item { Id = 10, Name = "Laptop", LocationId = 1 };

            var fromLocation = new Location { Id = 1, Name = "Warehouse A", Address = "Address 1" };
            var toLocation = new Location { Id = 2, Name = "Warehouse B", Address = "Address 2" };

            _itemRepositoryMock.Setup(repo => repo.GetByIdAsync(transactionCreateDto.ItemId))
                .ReturnsAsync(item);

            _locationRepositoryMock.Setup(repo => repo.GetByIdAsync(transactionCreateDto.FromLocationId))
                .ReturnsAsync(fromLocation);

            _locationRepositoryMock.Setup(repo => repo.GetByIdAsync(transactionCreateDto.ToLocationId))
                .ReturnsAsync(toLocation);

            _itemRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Item>()))
                .Returns(Task.CompletedTask);

            _transactionRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Transaction>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _transactionService.AddTransaction(transactionCreateDto);

            // Assert
            result.Should().NotBeNull();
            result.ItemId.Should().Be(transactionCreateDto.ItemId);
            result.FromLocationId.Should().Be(transactionCreateDto.FromLocationId);
            result.ToLocationId.Should().Be(transactionCreateDto.ToLocationId);
            result.UserId.Should().Be(transactionCreateDto.UserId);

            // Проверяем, что у предмета обновился LocationId
            item.LocationId.Should().Be(transactionCreateDto.ToLocationId);

            // Проверяем, что методы были вызваны 1 раз
            _itemRepositoryMock.Verify(repo => repo.UpdateAsync(item), Times.Once);
            _transactionRepositoryMock.Verify(repo => repo.AddAsync(It.Is<Transaction>(t =>
                t.ItemId == transactionCreateDto.ItemId &&
                t.FromLocationId == transactionCreateDto.FromLocationId &&
                t.ToLocationId == transactionCreateDto.ToLocationId &&
                t.UserId == transactionCreateDto.UserId
            )), Times.Once);
        }


        [Fact]
        public async Task AddTransaction_ShouldThrowException_WhenItemNotFound()
        {
            // Arrange
            var transactionCreateDto = new TransactionCreateDTO
            { 
                ItemId = 99,
                FromLocationId = 1,
                ToLocationId = 2,
                UserId = "user1"
            };
            var transaction = new Transaction
            {
                ItemId = transactionCreateDto.ItemId,
                FromLocationId = transactionCreateDto.FromLocationId,
                ToLocationId = transactionCreateDto.ToLocationId,
                UserId = transactionCreateDto.UserId,
                Timestamp = transactionCreateDto.Timestamp
            };
            _itemRepositoryMock.Setup(repo => repo.GetByIdAsync(transaction.ItemId))
                .ReturnsAsync((Item)null);
            
            // Act
            var act = async () => await _transactionService.AddTransaction(transactionCreateDto);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Item not found.");
            _transactionRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Transaction>()), Times.Never);
        }

        [Fact]
        public async Task AddTransaction_ShouldThrowException_WhenLocationNotFound()
        {
            // Arrange
            var transactionCreateDto = new TransactionCreateDTO
            {
                ItemId = 99,
                FromLocationId = 1,
                ToLocationId = 2,
                UserId = "user1"
            };
            var transaction = new Transaction
            {
                ItemId = transactionCreateDto.ItemId,
                FromLocationId = transactionCreateDto.FromLocationId,
                ToLocationId = transactionCreateDto.ToLocationId,
                UserId = transactionCreateDto.UserId,
                Timestamp = transactionCreateDto.Timestamp
            };

            var item = new Item { Id = 10, Name = "Laptop", LocationId = 1 };

            _itemRepositoryMock.Setup(repo => repo.GetByIdAsync(transaction.ItemId))
                .ReturnsAsync(item);

            _locationRepositoryMock.Setup(repo => repo.GetByIdAsync(transaction.FromLocationId))
                .ReturnsAsync((Location)null);

            // Act
            var act = async () => await _transactionService.AddTransaction(transactionCreateDto);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Invalid locations.");
            _transactionRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Transaction>()), Times.Never);
        }
    }
}
