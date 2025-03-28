﻿using InventoryManagement.Application.Services;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Interfaces;
using Moq;
using System.Security.Claims;

namespace InventoryManagement.Tests.UnitTests.Services
{
    public class AccountServiceTests
    {
        private readonly Mock<IAccountRepository> _accountRepositoryMock;
        private readonly Mock<IAuth0Repository> _auth0RepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly AccountService _accountService;

        public AccountServiceTests()
        {
            _accountRepositoryMock = new Mock<IAccountRepository>();
            _auth0RepositoryMock = new Mock<IAuth0Repository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _accountService = new AccountService(_accountRepositoryMock.Object, _auth0RepositoryMock.Object, _userRepositoryMock.Object);
        }

        [Fact]
        public async Task SyncUserAsync_ShouldCreateUser_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = "test-user-id";
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim("nickname", "test@example.com"),
                new Claim("https://your-app.com/first_name", "John"),
                new Claim("https://your-app.com/last_name", "Doe"),
                new Claim("sid", "hashed-password")
            };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));
            _userRepositoryMock.Setup(us => us.GetByIdAsync(userId)).ReturnsAsync((User)null);

            // Act
            await _accountService.SyncUserAsync(claimsPrincipal);

            // Assert
            _userRepositoryMock.Verify(us => us.AddAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task SyncUserAsync_ShouldNotCreateUser_WhenUserExists()
        {
            // Arrange
            var userId = "test-user-id";
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "mock"));
            _userRepositoryMock.Setup(us => us.GetByIdAsync(userId)).ReturnsAsync(new User { Id = userId });

            // Act
            await _accountService.SyncUserAsync(claimsPrincipal);

            // Assert
            _userRepositoryMock.Verify(us => us.AddAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task AssignRoleAfterLoginAsync_ShouldAssignRole_WhenNoExistingRoles()
        {
            // Arrange
            var userId = "test-user-id";
            var role = "Admin";
            _accountRepositoryMock.Setup(repo 
                => repo.GetUserRolesAsync(userId)).ReturnsAsync(new List<string>());
            _auth0RepositoryMock.Setup(repo 
                => repo.GetRoleId(role)).Returns("role-id");
            var claimsPrincipal = new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "mock"));

            // Act
            await _accountService.AssignRoleAfterLoginAsync(claimsPrincipal, role);

            // Assert
            _accountRepositoryMock.Verify(repo => repo.AssignRoleAsync(userId, "role-id"), Times.Once);
        }

        [Fact]
        public async Task AssignRoleAfterLoginAsync_ShouldNotAssignRole_WhenUserAlreadyHasRoles()
        {
            // Arrange
            var userId = "test-user-id";
            var role = "Admin";
            _accountRepositoryMock.Setup(repo 
                => repo.GetUserRolesAsync(userId)).ReturnsAsync(new List<string> { role });
            var claimsPrincipal = new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "mock"));

            // Act
            await _accountService.AssignRoleAfterLoginAsync(claimsPrincipal, role);

            // Assert
            _accountRepositoryMock.Verify(repo 
                => repo.AssignRoleAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AssignRoleAfterLoginAsync_ShouldThrowException_WhenUserIdNotFound()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[0], "mock"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() 
                => _accountService.AssignRoleAfterLoginAsync(claimsPrincipal, "Admin"));
        }

        [Fact]
        public async Task AssignRoleAfterLoginAsync_ShouldThrowException_WhenRoleNotFound()
        {
            // Arrange
            var userId = "test-user-id";
            _accountRepositoryMock.Setup(repo 
                => repo.GetUserRolesAsync(userId)).ReturnsAsync(new List<string>());
            _auth0RepositoryMock.Setup(repo 
                => repo.GetRoleId(It.IsAny<string>())).Returns((string)null);
            var claimsPrincipal = new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "mock"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() 
                => _accountService.AssignRoleAfterLoginAsync(claimsPrincipal, "InvalidRole"));
        } 

        [Fact]
        public async Task SyncUserAsync_ShouldNotCreateUser_WhenUserIsNotAuthenticated()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

            // Act
            await _accountService.SyncUserAsync(claimsPrincipal);

            // Assert
            _userRepositoryMock.Verify(us => us.AddAsync(It.IsAny<User>()), Times.Never);
        }

    }
}
