using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using InventoryManagement.Domain.Entities.Auth0;
using InventoryManagement.Domain.Interfaces;
using System.Net.Http.Headers;

namespace Infrastructure.Repositories
{
    public class Auth0Repository : IAuth0Repository
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly string _auth0Domain;

        public Auth0Repository(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
            _auth0Domain = _config["Auth0:Domain"];
        }

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
            return tokenResponse?.GetValueOrDefault("access_token") ?? throw new Exception("Ответ не содержит access_token");
        }

        public async Task<List<Auth0UserResponse>> GetUsersAsync()
        {
            var url = $"https://{_auth0Domain}/api/v2/users";
            var accessToken = await GetAccessTokenAsync();

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.GetAsync(url);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Ошибка получения пользователей: {response.StatusCode}, {responseString}");

            return JsonConvert.DeserializeObject<List<Auth0UserResponse>>(responseString);
        }

        public async Task<Auth0UserResponse> CreateUserAsync(UserCreateDTO request)
        {
            var url = $"https://{_auth0Domain}/api/v2/users";
            var accessToken = await GetAccessTokenAsync();

            var payload = new
            {
                email = request.Email,
                password = request.Password,
                connection = "Username-Password-Authentication",
                user_metadata = new { first_name = request.FirstName, last_name = request.LastName },
                app_metadata = new { role = request.Role }
            };

            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Ошибка создания пользователя: {response.StatusCode}, {responseString}");

            return JsonConvert.DeserializeObject<Auth0UserResponse>(responseString);
        }

        public async Task<bool> UpdateUserAsync(string userId, UpdateUserRequest request)
        {
            var url = $"https://{_auth0Domain}/api/v2/users/{userId}";
            var accessToken = await GetAccessTokenAsync();

            var updatePayload = new
            {
                email = request.Email,
                user_metadata = new { first_name = request.FirstName, last_name = request.LastName },
                app_metadata = new { role = request.Role }
            };

            var content = new StringContent(JsonConvert.SerializeObject(updatePayload), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.PatchAsync(url, content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            var url = $"https://{_auth0Domain}/api/v2/users/{userId}";
            var accessToken = await GetAccessTokenAsync();

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.DeleteAsync(url);

            return response.IsSuccessStatusCode;
        }
    }

}
