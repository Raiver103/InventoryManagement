using InventoryManagement.Application.DTOs.User;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Application.Services;
using InventoryManagement.WEB.Controollers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

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
            Assert.Equal("https://localhost:7025/afterlogin", redirectResult.Url);
        }

        [Fact]
        public async Task Login_ShouldRedirectToLoginPage()
        {
            // Act
            var result = await _controller.Login();

            // Assert
            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal("https://localhost:7025/login", redirectResult.Url);
        }

        [Fact]
        public async Task Logout_ShouldSignOutUser()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                .Returns("https://localhost:7025/home");

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
