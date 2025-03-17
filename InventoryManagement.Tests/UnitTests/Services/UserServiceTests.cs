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

namespace InventoryManagement.Tests.UnitTests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _userService = new UserService(_userRepositoryMock.Object);
        }

        [Fact]
        public async Task GetUserById_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var userId = "123";
            var expectedUser = new User { Id = userId, FirstName = "John", LastName = "Doe" };

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(expectedUser);

            // Act
            var result = await _userService.GetUserById(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedUser);
        }

        [Fact]
        public async Task GetUserById_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _userService.GetUserById("non-existent-id");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AddUser_ShouldCallRepositoryOnce()
        {
            // Arrange
            var newUser = new User { Id = "456", FirstName = "Jane", LastName = "Doe" };

            // Act
            await _userService.AddUser(newUser);

            // Assert
            _userRepositoryMock.Verify(repo => repo.AddAsync(newUser), Times.Once);
        }

        [Fact]
        public async Task UpdateUser_ShouldCallRepositoryOnce()
        {
            // Arrange
            var existingUser = new User { Id = "789", FirstName = "Updated", LastName = "User" };

            // Act
            await _userService.UpdateUser(existingUser);

            // Assert
            _userRepositoryMock.Verify(repo => repo.UpdateAsync(existingUser), Times.Once);
        }

        [Fact]
        public async Task DeleteUser_ShouldCallRepositoryOnce()
        {
            // Arrange
            var userId = "999";

            // Act
            await _userService.DeleteUser(userId);

            // Assert
            _userRepositoryMock.Verify(repo => repo.DeleteAsync(userId), Times.Once);
        }
    }
}
