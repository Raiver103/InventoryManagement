using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

public class Auth0Service
{
    private readonly HttpClient _httpClient;
    private string _accessToken;
    private readonly string _domain;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _audience;

    public Auth0Service(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _domain = "dev-kt11cdvtf78rhwal.us.auth0.com";
        _clientId = "G27lTC3kt01nxUhVfZZdQkhOkhoAHFQS";
        _clientSecret = "vLlnkFMdh20PAcBVuZ1FyPvYGR4uBM42vSrYRwLWxjrlBg-lricTDKCIFHZyzNhh";
        _audience = $"https://inventory-api.com";
    }

    // 📌 Получение токена доступа
    private async Task<string> GetAccessToken()
    {
        if (!string.IsNullOrEmpty(_accessToken)) return _accessToken;

        var tokenRequest = new
        {
            client_id = _clientId,
            client_secret = _clientSecret,
            audience = _audience,
            grant_type = "client_credentials"
        };

        var response = await _httpClient.PostAsync($"https://{_domain}/oauth/token",
            new StringContent(JsonConvert.SerializeObject(tokenRequest), Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode)
            throw new Exception("Ошибка получения токена");

        var responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(
            await response.Content.ReadAsStringAsync());

        _accessToken = responseData["access_token"];
        return _accessToken;
    }

    // 📌 Создание пользователя
    public async Task<string> CreateUser(string email, string password)
    {
        await GetAccessToken();

        var newUser = new
        {
            email,
            password,
            connection = "Username-Password-Authentication"
        };

        var response = await _httpClient.PostAsync($"https://{_domain}/api/v2/users",
            new StringContent(JsonConvert.SerializeObject(newUser), Encoding.UTF8, "application/json"));

        return await response.Content.ReadAsStringAsync();
    }

    // 📌 Получение списка пользователей
    public async Task<string> GetUsers()
    {
        await GetAccessToken();

        var response = await _httpClient.GetAsync($"https://{_domain}/api/v2/users");

        return await response.Content.ReadAsStringAsync();
    }

    // 📌 Обновление пользователя
    public async Task<string> UpdateUser(string userId, object updatedData)
    {
        await GetAccessToken();

        var response = await _httpClient.PatchAsync($"https://{_domain}/api/v2/users/{userId}",
            new StringContent(JsonConvert.SerializeObject(updatedData), Encoding.UTF8, "application/json"));

        return await response.Content.ReadAsStringAsync();
    }

    // 📌 Удаление пользователя
    public async Task<string> DeleteUser(string userId)
    {
        await GetAccessToken();

        var response = await _httpClient.DeleteAsync($"https://{_domain}/api/v2/users/{userId}");

        return response.IsSuccessStatusCode ? "Пользователь удален" : "Ошибка удаления";
    }
}
