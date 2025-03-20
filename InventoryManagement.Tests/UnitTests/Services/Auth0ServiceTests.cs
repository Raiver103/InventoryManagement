using InventoryManagement.Domain.Entities.Auth0;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Interfaces;
using Moq;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Infrastructure.Repositories;

namespace InventoryManagement.Tests.UnitTests.Services
{
    public class Auth0ServiceTests
    {
        private readonly Mock<IAuth0Repository> _auth0RepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Auth0Service _auth0Service;

        public Auth0ServiceTests()
        {
            _auth0RepositoryMock = new Mock<IAuth0Repository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _auth0Service = new Auth0Service(_auth0RepositoryMock.Object, _userRepositoryMock.Object);
        }

        [Fact]
        public async Task GetAccessTokenAsync_ShouldReturnToken()
        {
            _auth0RepositoryMock.Setup(repo 
                => repo.GetAccessTokenAsync()).ReturnsAsync("test-token");

            var token = await _auth0Service.GetAccessTokenAsync();

            Assert.Equal("test-token", token);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldCreateUser()
        {
            var request = new CreateUserRequest 
            { 
                FirstName = "John", 
                LastName = "Doe", 
                Role = "Admin", 
                Email = "test@example.com", 
                Password = "password123" 
            };

            var auth0User = new Auth0UserResponse { Id = "12345" };

            _auth0RepositoryMock.Setup(repo => repo.CreateUserAsync(request)).ReturnsAsync(auth0User);
            _userRepositoryMock.Setup(us => us.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            var newUser = await _auth0Service.CreateUserAsync(request);

            Assert.Equal(auth0User.Id, newUser.Id);
            Assert.Equal(request.FirstName, newUser.FirstName);
            Assert.Equal(request.LastName, newUser.LastName);
            Assert.Equal(request.Email, newUser.Email);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldUpdateUser_WhenUserExists()
        {
            var request = new UpdateUserRequest 
            { 
                FirstName = "Jane", 
                LastName = "Smith", 
                Role = "User", 
                Email = "jane@example.com" 
            };

            var auth0User = new Auth0UserResponse { Id = "12345" };

            var existingUser = new User 
            { 
                Id = "12345", 
                FirstName = "OldName", 
                LastName = "OldLast", 
                Email = "old@example.com", 
                Role = "OldRole" 
            };

            _auth0RepositoryMock.Setup(repo 
                => repo.UpdateUserAsync("12345", request)).ReturnsAsync(auth0User);
            _userRepositoryMock.Setup(us 
                => us.GetByIdAsync("12345")).ReturnsAsync(existingUser);
            _userRepositoryMock.Setup(us 
                => us.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            var updatedUser = await _auth0Service.UpdateUserAsync("12345", request);

            Assert.Equal(request.FirstName, updatedUser.FirstName);
            Assert.Equal(request.LastName, updatedUser.LastName);
            Assert.Equal(request.Email, updatedUser.Email);
            Assert.Equal(request.Role, updatedUser.Role);
        }

        [Fact]
        public async Task GetUsersAsync_ShouldReturnUsers()
        {
            var users = new List<Auth0UserResponse> 
            { 
                new Auth0UserResponse { Id = "123" }, 
                new Auth0UserResponse { Id = "456" } 
            };

            _auth0RepositoryMock.Setup(repo 
                => repo.GetUsersAsync()).ReturnsAsync(users);

            var result = await _auth0Service.GetUsersAsync();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldDeleteUser()
        {
            _auth0RepositoryMock.Setup(repo 
                => repo.DeleteUserAsync("12345")).Returns(Task.CompletedTask);
            _userRepositoryMock.Setup(us 
                => us.DeleteAsync("12345")).Returns(Task.CompletedTask);

            await _auth0Service.DeleteUserAsync("12345");

            _auth0RepositoryMock.Verify(repo
                => repo.DeleteUserAsync("12345"), Times.Once);
            _userRepositoryMock.Verify(us 
                => us.DeleteAsync("12345"), Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldThrowException_WhenAuth0UserCreationFails()
        {
            // Arrange
            var request = new CreateUserRequest 
            {
                FirstName = "John", 
                LastName = "Doe", 
                Role = "Admin", 
                Email = "test@example.com", 
                Password = "password123" 
            };

            _auth0RepositoryMock.Setup(repo 
                => repo.CreateUserAsync(request)).ReturnsAsync((Auth0UserResponse)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _auth0Service.CreateUserAsync(request));
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldThrowException_WhenUserNotFound()
        {
            // Arrange
            var request = new UpdateUserRequest 
            { 
                FirstName = "Jane", 
                LastName = "Smith", 
                Role = "User", 
                Email = "jane@example.com" 
            };

            _userRepositoryMock.Setup(us 
                => us.GetByIdAsync("non-existent-id")).ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _auth0Service.UpdateUserAsync("non-existent-id", request));
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldNotCallAuth0Delete_WhenDatabaseDeleteFails()
        {
            // Arrange
            _userRepositoryMock.Setup(us 
                => us.DeleteAsync("12345")).ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _auth0Service.DeleteUserAsync("12345"));
            _auth0RepositoryMock.Verify(repo => repo.DeleteUserAsync(It.IsAny<string>()), Times.Never);
        }

    }

}
