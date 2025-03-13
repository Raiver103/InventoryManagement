using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System;
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using InventoryManagement.Application.DTOs.User;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using InventoryManagement.Application.Services;
using InventoryManagement.Domain.Entities;
using AutoMapper;

namespace InventoryManagement.WEB.Controollers
{
    public class AccountController : Controller
    {
        private readonly UserService _userService;

        public AccountController(UserService userService)
        {
            _userService = userService; 
        }

        [HttpGet("/Account/Login")]
        public IActionResult LoginUserRedirect(string returnUrl = "/")
        {
            return Redirect("https://localhost:7025/login");
        }
        [HttpGet("/Account/AccessDenied")]
        public IActionResult LoginUserRedirectAccessDenied(string returnUrl = "/")
        {
            return Redirect("https://localhost:7025/login");
        }

        [HttpGet("/Auth/Login")]
        public async Task LoginUser(string returnUrl = "/")
        { 
            var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
            // Indicate here where Auth0 should redirect the user after a login.
            // Note that the resulting absolute Uri must be added to the
            // **Allowed Callback URLs** settings for the app.
            .WithRedirectUri(returnUrl)
            .Build();

            await HttpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
        }

        [HttpGet("/Account/Signup")]
        public async Task Signup(string returnUrl = "/")
        {
            var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
                .WithParameter("screen_hint", "signup")
                .WithRedirectUri(returnUrl)
            .Build();

            await HttpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
            
        }

        [Authorize]
        [HttpGet("/Account/Logout")]
        public async Task Logout()
        {
            var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
            // Indicate here where Auth0 should redirect the user after a logout.
            // Note that the resulting absolute Uri must be added to the
            // **Allowed Logout URLs** settings for the app.
                .WithRedirectUri(Url.Action("Index", "Home"))
                .Build();

            await HttpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); 

        }

        [Authorize]
        [HttpGet("/Account/Profile")]
        public async Task<IActionResult> Profile()
        {

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            // In your database, check if user exist's or not.
            // if(userNotExists)=> create new entry with 'userId'. You can alsow save Other user info.
            await SyncUser();

            var userProfile = new UserProfile
            {
                FirstName = User.FindFirst("https://your-app.com/first_name")?.Value,
                LastName = User.FindFirst("https://your-app.com/last_name")?.Value,
                EmailAddress = User.FindFirst(c => c.Type == "nickname")?.Value,
                ProfileImage = User.Claims.FirstOrDefault(c => c.Type == "picture")?.Value
            };
            return View(userProfile);
        }
        private async Task SyncUser()
        {
            if (!User.Identity.IsAuthenticated)
                return;

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return; // Email обязателен для идентификации
            }

            var email = User.FindFirst(c => c.Type == "nickname")?.Value ?? "Не указано"; 
            var firstName = User.FindFirst("https://your-app.com/first_name")?.Value ?? "Не указано";
            var lastName = User.FindFirst("https://your-app.com/last_name")?.Value ?? "Не указано";
            var password = User.FindFirst(c => c.Type == "sid")?.Value ?? "Не указано";

            var existingUser = await _userService.GetUserById(userId);

            if (existingUser == null)
            {
                var newUser = new User
                {
                    Id = userId,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName, 
                    Role = "Employee",
                    PasswordHash = password
                };

                await _userService.AddUser(newUser);
            }
        }

    }

    public class UserProfile
    {
        public string EmailAddress { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string ProfileImage { get; set; }
        public string UserId { get; set; }
    }
}
