using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

[Route("api/auth0")]
[ApiController]
public class Auth0Controller : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly string _auth0Domain;

    public Auth0Controller(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
        _auth0Domain = _config["Auth0:Domain"];
    }

    // 📌 Получение токена (динамически)
    [HttpPost("get-access-token")]
    public async Task<string> GetAccessTokenAsync()
    {
        var clientId = _config["Auth0M2M:ClientId"];
        var clientSecret = _config["Auth0M2M:ClientSecret"]; 
        var audience = "https://dev-kt11cdvtf78rhwal.us.auth0.com/api/v2/";

        var url = $"https://dev-kt11cdvtf78rhwal.us.auth0.com/oauth/token";
        var payload = new
        {
            client_id = "G27lTC3kt01nxUhVfZZdQkhOkhoAHFQS", 
            client_secret = "vLlnkFMdh20PAcBVuZ1FyPvYGR4uBM42vSrYRwLWxjrlBg-lricTDKCIFHZyzNhh",
            audience = "https://dev-kt11cdvtf78rhwal.us.auth0.com/api/v2/",
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

        if (tokenResponse != null && tokenResponse.ContainsKey("access_token"))
        {
            return tokenResponse["access_token"];
        }

        throw new Exception("Ответ не содержит access_token");
    }
    // 📌 Создание пользователя
    [HttpPost("create-user")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var url = $"https://{_auth0Domain}/api/v2/users";
        var accessToken = await GetAccessTokenAsync();
        Console.WriteLine(accessToken);
        if (string.IsNullOrEmpty(accessToken))
            return Unauthorized("Не удалось получить токен");

        var payload = new
        {
            email = request.Email,
            password = request.Password,
            connection = "Username-Password-Authentication"
        };

        var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

        // ✅ Исправлен формат Authorization заголовка
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.PostAsync(url, content);
        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return BadRequest($"Ошибка: {response.StatusCode}, {responseString}");

        return Ok(JsonConvert.DeserializeObject(responseString));
    }


    // 📌 Обновление пользователя
    [HttpPatch("update-user/{userId}")]
    public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserRequest request)
    {
        var url = $"https://{_auth0Domain}/api/v2/users/{userId}";
        var accessToken = await GetAccessTokenAsync();
        if (accessToken == null) return Unauthorized("Не удалось получить токен");

        var payload = new { email = request.Email };
        var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.PatchAsync(url, content);
        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return BadRequest(responseString);

        return Ok(JsonConvert.DeserializeObject(responseString));
    }

    // 📌 Удаление пользователя
    [HttpDelete("delete-user/{userId}")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var url = $"https://{_auth0Domain}/api/v2/users/{userId}";
        var accessToken = await GetAccessTokenAsync();
        if (accessToken == null) return Unauthorized("Не удалось получить токен");

        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.DeleteAsync(url);
        if (!response.IsSuccessStatusCode)
            return BadRequest(await response.Content.ReadAsStringAsync());

        return Ok(new { message = "User deleted successfully" });
    }
}

// DTO-классы для запросов
public class CreateUserRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class UpdateUserRequest
{
    public string Email { get; set; }
}
