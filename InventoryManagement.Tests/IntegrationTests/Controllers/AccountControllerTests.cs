﻿using InventoryManagement.Application.DTOs.User;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.WEB.Controllers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Security.Claims;

namespace InventoryManagement.Tests.IntegrationTests.Controllers
{
    [Collection("Sequential")]
    public class AccountControllerTests
    {
        private readonly Mock<IAccountService> _accountServiceMock;
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            _accountServiceMock = new Mock<IAccountService>();
            _controller = new AccountController(_accountServiceMock.Object);
        }

        [Fact]
        public async Task AfterLogin_ShouldRedirectToAfterLoginPage()
        {
            // Act
            var result = await _controller.AfterLogin();

            // Assert
            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal("http://localhost:5000/afterlogin", redirectResult.Url);
        }

        [Fact]
        public async Task Login_ShouldRedirectToLoginPage()
        {
            // Act
            var result = await _controller.Login();

            // Assert
            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal("http://localhost:5000/login", redirectResult.Url);
        }

        [Fact]
        public async Task Logout_ShouldSignOutUser()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                .Returns("http://localhost:5000/home");

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            _controller.Url = urlHelperMock.Object;

            var services = new ServiceCollection();
            services.AddSingleton(Mock.Of<IAuthenticationService>());
            httpContext.RequestServices = services.BuildServiceProvider();

            // Act
            await _controller.Logout();

            // Assert
            Assert.True(httpContext.Response.StatusCode == 200);
        }


        [Fact]
        public async Task Profile_ShouldReturnUserProfileView()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "123"),
                new Claim("https://your-app.com/first_name", "John"),
                new Claim("https://your-app.com/last_name", "Doe"),
                new Claim("nickname", "johndoe"),
                new Claim("picture", "https://example.com/profile.jpg")
            };
            var identity = new ClaimsIdentity(claims, "mock");
            var user = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext { User = user };
            _controller.ControllerContext = new ControllerContext() { HttpContext = httpContext };

            // Act
            var result = await _controller.Profile();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UserProfileDto>(viewResult.Model);
            Assert.Equal("John", model.FirstName);
            Assert.Equal("Doe", model.LastName);
            Assert.Equal("johndoe", model.EmailAddress);
            Assert.Equal("https://example.com/profile.jpg", model.ProfileImage);
        }
    }
}
