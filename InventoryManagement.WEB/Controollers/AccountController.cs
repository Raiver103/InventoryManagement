﻿using Microsoft.AspNetCore.Authentication.Cookies;
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
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using Infrastructure.Repositories;
using InventoryManagement.Domain.Interfaces;
using InventoryManagement.Application.Models.Auth0;

namespace InventoryManagement.WEB.Controollers
{
    public class AccountController : Controller
    {
        private readonly UserService _userService;
        private readonly IAuth0Repository _auth0Repository;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly string _auth0Domain;

        public AccountController(IAuth0Repository auth0Repository, UserService userService, HttpClient httpClient, IConfiguration config)
        {
            _userService = userService;
            _httpClient = httpClient;
            _config = config;
            _auth0Domain = _config["Auth0:Domain"];
            _auth0Repository = auth0Repository;
        }

        [HttpGet("/Account/Login")]
        public async Task<IActionResult> LoginUserRedirect(string returnUrl = "/")
        {
            await AssignRoleAfterLogin();
            await SyncUser();
            return Redirect("https://localhost:7025/login");
        }

        [HttpGet("/Account/AccessDenied")]
        public IActionResult LoginUserRedirectAccessDenied(string returnUrl = "/")
        {
            return Redirect("https://localhost:7025/login");
        }

        [HttpGet("/Auth/Login")]
        public async Task LoginUser(string returnUrl = "/Account/Login")
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

            //await AssignRoleAfterLogin();
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
            //await SyncUser();

            var userProfile = new UserProfileDto
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
                    Role = "Admin",
                    PasswordHash = password
                };

                await _userService.AddUser(newUser);
            }
        }

        private async Task AssignRoleAfterLogin()
        {
            if (!User.Identity.IsAuthenticated) return;

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception("Ошибка: Не удалось получить идентификатор пользователя.");
            }

            // Проверяем, есть ли у пользователя уже роли
            var existingRoles = await GetUserRolesAsync(userId);
            if (existingRoles == null || !existingRoles.Any())
            {
                await AssignEmployeeRoleAsync(userId);
            }
        }

        private async Task AssignEmployeeRoleAsync(string userId)
        {
            var accessToken = await _auth0Repository.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken)) throw new Exception("Не удалось получить токен");

            // Получаем ID роли "Employee"
            var roleId = _auth0Repository.GetRoleId("Admin");
            if (string.IsNullOrEmpty(roleId)) throw new Exception("Ошибка: Роль 'Employee' не найдена");

            // Назначаем роль "Employee" через Auth0 API
            var roleAssignUrl = $"https://{_auth0Domain}/api/v2/users/{userId}/roles";
            var roleAssignPayload = new { roles = new List<string> { roleId } };
            var roleContent = new StringContent(JsonConvert.SerializeObject(roleAssignPayload), Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            var roleResponse = await _httpClient.PostAsync(roleAssignUrl, roleContent);

            if (!roleResponse.IsSuccessStatusCode)
                throw new Exception($"Ошибка назначения роли: {roleResponse.StatusCode}, {await roleResponse.Content.ReadAsStringAsync()}");
        }

        private async Task<List<string>> GetUserRolesAsync(string userId)
        {
            var accessToken = await _auth0Repository.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken)) throw new Exception("Не удалось получить токен");

            var rolesUrl = $"https://{_auth0Domain}/api/v2/users/{userId}/roles";

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.GetAsync(rolesUrl);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Ошибка получения ролей пользователя: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var roles = JsonConvert.DeserializeObject<List<Auth0Role>>(responseString);

            return roles?.Select(r => r.Name).ToList() ?? new List<string>();
        }
         
    } 
}
