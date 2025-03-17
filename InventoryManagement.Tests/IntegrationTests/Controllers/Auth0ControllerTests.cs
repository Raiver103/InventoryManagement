using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Entities.Auth0;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace InventoryManagement.Tests.IntegrationTests.Controllers
{
    [Collection("Sequential")]
    public class Auth0ControllerTests
    {
        private readonly Mock<IAuth0Service> _auth0ServiceMock;
        private readonly Auth0Controller _controller;

        public Auth0ControllerTests()
        {
            _auth0ServiceMock = new Mock<IAuth0Service>();
            _controller = new Auth0Controller(_auth0ServiceMock.Object);
        }

        [Fact]
        public async Task GetUsers_ShouldReturnListOfUsers()
        {
            // Arrange
            var users = new List<Auth0UserResponse>
    {
        new Auth0UserResponse { Id = "123", Email = "user1@example.com" },
        new Auth0UserResponse { Id = "456", Email = "user2@example.com" }
    };
            _auth0ServiceMock.Setup(service => service.GetUsersAsync()).ReturnsAsync(users);

            // Act
            var result = await _controller.GetUsersFromAuth0();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedUsers = okResult.Value as IEnumerable<object>; // изменяем тип
            Assert.NotNull(returnedUsers);
            Assert.Equal(2, returnedUsers.Count());
        }


        [Fact]
        public async Task CreateUser_ShouldReturnCreatedUser()
        {
            // Arrange
            var request = new CreateUserRequest { FirstName = "John", LastName = "Doe", Email = "test@example.com", Role = "Admin", Password = "password123" };
            var createdUser = new User { Id = "789", Email = "test@example.com" };
            _auth0ServiceMock.Setup(service => service.CreateUserAsync(request)).ReturnsAsync(createdUser);

            // Act
            var result = await _controller.CreateUser(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedUser = Assert.IsType<User>(okResult.Value);
            Assert.Equal("789", returnedUser.Id);
            Assert.Equal("test@example.com", returnedUser.Email);
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnUpdatedUser()
        {
            // Arrange
            var request = new UpdateUserRequest { FirstName = "Jane", LastName = "Smith", Email = "jane@example.com", Role = "User" };
            var updatedUser = new User { Id = "123", Email = "jane@example.com", FirstName = "Jane", LastName = "Smith", Role = "User" };
            _auth0ServiceMock.Setup(service => service.UpdateUserAsync("123", request)).ReturnsAsync(updatedUser);

            // Act
            var result = await _controller.UpdateUser("123", request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedUser = Assert.IsType<User>(okResult.Value);

            Assert.Equal("123", returnedUser.Id);
            Assert.Equal("jane@example.com", returnedUser.Email);
        }



        [Fact]
        public async Task DeleteUser_ShouldReturnNoContent()
        {
            // Arrange
            _auth0ServiceMock.Setup(service => service.DeleteUserAsync("123")).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteUser("123");

            // Assert
            Assert.IsType<NoContentResult>(result);
            _auth0ServiceMock.Verify(service => service.DeleteUserAsync("123"), Times.Once);
        }
    }
}
