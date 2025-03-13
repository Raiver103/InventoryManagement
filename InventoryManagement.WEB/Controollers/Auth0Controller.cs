using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using InventoryManagement.Application.Services;
using InventoryManagement.Infastructure.Persistence;
using BCrypt.Net;
using Azure.Core;

[Route("api/auth0")]
[ApiController]
public class Auth0Controller : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly string _auth0Domain;
    private readonly AppDbContext _context;

    public Auth0Controller(HttpClient httpClient, IConfiguration config, AppDbContext context)
    {
        _httpClient = httpClient;
        _config = config;
        _auth0Domain = _config["Auth0:Domain"];
        _context = context;
    }

    [HttpPost("get-access-token")]
    public async Task<string> GetAccessTokenAsync()
    {
        var url = $"https://{_auth0Domain}/oauth/token";
        var payload = new
        {
            client_id = _config["Auth0M2M:ClientId"],
            client_secret = _config["Auth0M2M:ClientSecret"],
            audience = $"https://{_auth0Domain}/api/v2/",
            grant_type = "client_credentials"
        };

        var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(url, content);
        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Ошибка получения токена: {response.StatusCode}, {responseString}");
        }

        var tokenResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);
        return tokenResponse != null && tokenResponse.ContainsKey("access_token") ? tokenResponse["access_token"] : throw new Exception("Ответ не содержит access_token");
    }

    [HttpPost("create-user")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var url = $"https://{_auth0Domain}/api/v2/users";
        var accessToken = await GetAccessTokenAsync();
        if (string.IsNullOrEmpty(accessToken)) return Unauthorized("Не удалось получить токен");

        var auth0Payload = new
        {
            email = request.Email,
            password = request.Password,
            connection = "Username-Password-Authentication",
            user_metadata = new
            {
                first_name = request.FirstName,
                last_name = request.LastName
            },
            app_metadata = new
            {
                role = request.Role
            }
        };

        var content = new StringContent(JsonConvert.SerializeObject(auth0Payload), Encoding.UTF8, "application/json");
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _httpClient.PostAsync(url, content);
        var responseString = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode) return BadRequest($"Ошибка создания пользователя в Auth0: {response.StatusCode}, {responseString}");

        var createdUser = JsonConvert.DeserializeObject<Auth0UserResponse>(responseString);
        var auth0UserId = createdUser.Id;
        var roleId = GetRoleId(request.Role);
        if (string.IsNullOrEmpty(roleId)) return BadRequest("Ошибка: Указанная роль не найдена");

        var roleAssignUrl = $"https://{_auth0Domain}/api/v2/users/{auth0UserId}/roles";
        var roleAssignPayload = new { roles = new List<string> { roleId } };
        var roleContent = new StringContent(JsonConvert.SerializeObject(roleAssignPayload), Encoding.UTF8, "application/json");
        var roleResponse = await _httpClient.PostAsync(roleAssignUrl, roleContent);
        if (!roleResponse.IsSuccessStatusCode) return BadRequest($"Ошибка назначения роли: {roleResponse.StatusCode}, {await roleResponse.Content.ReadAsStringAsync()}");

        var newUser = new User
        {
            Id = auth0UserId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = request.Role,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            ManagedItems = new List<Item>()
        };
        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();
        return Ok(newUser);
    }

    [HttpPatch("update-user/{userId}")]
    public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserRequest request)
    {
        var url = $"https://{_auth0Domain}/api/v2/users/{userId}";
        var accessToken = await GetAccessTokenAsync();
        if (string.IsNullOrEmpty(accessToken)) return Unauthorized("Не удалось получить токен");

        var updatePayload = new
        {
            email = request.Email,
            user_metadata = new
            {
                first_name = request.FirstName,
                last_name = request.LastName
            },
            app_metadata = new
            {
                role = request.Role
            }
        };

        var content = new StringContent(JsonConvert.SerializeObject(updatePayload), Encoding.UTF8, "application/json");
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _httpClient.PatchAsync(url, content);
        if (!response.IsSuccessStatusCode) return BadRequest($"Ошибка обновления пользователя в Auth0: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return NotFound("Пользователь не найден в базе данных");

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.Role = request.Role;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Пользователь успешно обновлен", user });
    }
    private string GetRoleId(string roleName)
    {
        var roles = new Dictionary<string, string>
        {
            { "Admin", "rol_6JJSMFRpkvMKUCr8" },
            { "Employee", "rol_1zXKaYdC0yYkur5R" },
            { "Manager", "rol_MroJU4loFATOSJTp" }
        };

        return roles.ContainsKey(roleName) ? roles[roleName] : null;
    }
    [HttpGet("get-users")]
    public async Task<IActionResult> GetUsersFromAuth0()
    {
        var url = $"https://{_auth0Domain}/api/v2/users";
        var accessToken = await GetAccessTokenAsync();

        if (string.IsNullOrEmpty(accessToken))
            return Unauthorized("Не удалось получить токен");

        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.GetAsync(url);
        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return BadRequest($"Ошибка получения пользователей из Auth0: {response.StatusCode}, {responseString}");

        var users = JsonConvert.DeserializeObject<List<Auth0UserResponse>>(responseString);

        // 🔹 Обрабатываем метаданные (firstName, lastName, role)
        var formattedUsers = users.Select(u => new
        {
            Id = u.Id,
            Email = u.Email,
            FirstName = u.Metadata?.FirstName ?? "Не указано",
            LastName = u.Metadata?.LastName ?? "Не указано",
            Role = u.AppMetadata?.Role ?? "Не указано" // 👈 Теперь role загружается
        });

        return Ok(formattedUsers);
    }

    [HttpDelete("delete-user/{userId}")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var url = $"https://{_auth0Domain}/api/v2/users/{userId}";
        var accessToken = await GetAccessTokenAsync();

        if (string.IsNullOrEmpty(accessToken))
            return Unauthorized("Не удалось получить токен");

        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.DeleteAsync(url);

        if (!response.IsSuccessStatusCode)
            return BadRequest($"Ошибка удаления пользователя в Auth0: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        return Ok(new { message = "Пользователь успешно удален" });
    }
}
 

public class CreateUserRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Role { get; set; }
}

public class UpdateUserRequest
{
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Role { get; set; }
}
public class Auth0UserResponse
{
    [JsonProperty("user_id")]
    public string Id { get; set; }

    [JsonProperty("email")]
    public string Email { get; set; }

    [JsonProperty("user_metadata")]
    public UserMetadata Metadata { get; set; }

    [JsonProperty("app_metadata")]
    public AppMetadata AppMetadata { get; set; } // 👈 Добавил app_metadata
    public string FirstName => Metadata?.FirstName;
    public string LastName => Metadata?.LastName;
    public string Role => AppMetadata?.Role;  // ✅ Теперь Role подтягивается
}
public class UserMetadata
{
    [JsonProperty("first_name")]
    public string FirstName { get; set; }

    [JsonProperty("last_name")]
    public string LastName { get; set; }
}
public class AppMetadata
{
    [JsonProperty("role")]
    public string Role { get; set; }
}