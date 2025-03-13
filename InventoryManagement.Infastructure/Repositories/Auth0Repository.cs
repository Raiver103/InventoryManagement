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
            return tokenResponse != null && tokenResponse.ContainsKey("access_token") ? tokenResponse["access_token"] : throw new Exception("Ответ не содержит access_token");
        }

        public async Task<Auth0UserResponse> CreateUserAsync(UserCreateDTO request)
        {
            var url = $"https://{_auth0Domain}/api/v2/users";
            var accessToken = await GetAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken)) throw new Exception("Не удалось получить токен");

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
            if (!response.IsSuccessStatusCode) throw new Exception($"Ошибка создания пользователя в Auth0: {response.StatusCode}, {responseString}");

            var createdUser = JsonConvert.DeserializeObject<Auth0UserResponse>(responseString);
            var auth0UserId = createdUser.Id;
            var roleId = GetRoleId(request.Role);
            if (string.IsNullOrEmpty(roleId)) throw new Exception("Ошибка: Указанная роль не найдена");

            var roleAssignUrl = $"https://{_auth0Domain}/api/v2/users/{auth0UserId}/roles";
            var roleAssignPayload = new { roles = new List<string> { roleId } };
            var roleContent = new StringContent(JsonConvert.SerializeObject(roleAssignPayload), Encoding.UTF8, "application/json");
            var roleResponse = await _httpClient.PostAsync(roleAssignUrl, roleContent);
            if (!roleResponse.IsSuccessStatusCode) throw new Exception($"Ошибка назначения роли: {roleResponse.StatusCode}, {await roleResponse.Content.ReadAsStringAsync()}");

            return createdUser;
        }

        public async Task<Auth0UserResponse> UpdateUserAsync(string userId, UpdateUserRequest request)
        {
            var url = $"https://{_auth0Domain}/api/v2/users/{userId}";
            var accessToken = await GetAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken)) throw new Exception("Не удалось получить токен");

            var updatePayload = new Dictionary<string, object>
            {
                { "user_metadata", new { first_name = request.FirstName, last_name = request.LastName } },
                { "app_metadata", new { role = request.Role } }
            };

            // Добавляем email, если это не Google/GitHub пользователь
            if (!(userId.Contains("google-oauth2")))
            {
                updatePayload["email"] = request.Email;
            }

            var content = new StringContent(JsonConvert.SerializeObject(updatePayload), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.PatchAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();
            var auth0User = JsonConvert.DeserializeObject<Auth0UserResponse>(responseString);
            var auth0UserId = auth0User.Id;

            if (!response.IsSuccessStatusCode) throw new Exception($"Ошибка обновления пользователя в Auth0: {response.StatusCode}, {responseString}");

            var newRoleId = GetRoleId(request.Role);
            if (string.IsNullOrEmpty(newRoleId)) throw new Exception("Ошибка: Указанная роль не найдена");

            var rolesUrl = $"https://{_auth0Domain}/api/v2/users/{userId}/roles";
            var rolesResponse = await _httpClient.GetAsync(rolesUrl);
            var rolesResponseString = await rolesResponse.Content.ReadAsStringAsync();

            if (!rolesResponse.IsSuccessStatusCode) throw new Exception($"Ошибка получения текущих ролей пользователя: {rolesResponse.StatusCode}, {rolesResponseString}");

            var currentRoles = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(rolesResponseString);
            var currentRoleIds = currentRoles?.Select(r => r["id"]).ToList() ?? new List<string>();

            if (!currentRoleIds.Contains(newRoleId))
            {
                if (currentRoleIds.Any())
                {
                    var removeRolesPayload = new { roles = currentRoleIds };
                    var removeRolesContent = new StringContent(JsonConvert.SerializeObject(removeRolesPayload), Encoding.UTF8, "application/json");
                    var removeRolesResponse = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete, rolesUrl) { Content = removeRolesContent });

                    if (!removeRolesResponse.IsSuccessStatusCode) throw new Exception($"Ошибка удаления старых ролей: {removeRolesResponse.StatusCode}, {await removeRolesResponse.Content.ReadAsStringAsync()}");
                }

                var addRolesPayload = new { roles = new List<string> { newRoleId } };
                var addRolesContent = new StringContent(JsonConvert.SerializeObject(addRolesPayload), Encoding.UTF8, "application/json");
                var addRolesResponse = await _httpClient.PostAsync(rolesUrl, addRolesContent);

                if (!addRolesResponse.IsSuccessStatusCode) throw new Exception($"Ошибка назначения новой роли: {addRolesResponse.StatusCode}, {await addRolesResponse.Content.ReadAsStringAsync()}");
            }

            return auth0User;
        }

        public async Task<List<Auth0UserResponse>> GetUsersAsync()
        {
            var url = $"https://{_auth0Domain}/api/v2/users";
            var accessToken = await GetAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken)) throw new Exception("Не удалось получить токен");

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.GetAsync(url);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode) throw new Exception($"Ошибка получения пользователей из Auth0: {response.StatusCode}, {responseString}");

            return JsonConvert.DeserializeObject<List<Auth0UserResponse>>(responseString);
        }

        public async Task DeleteUserAsync(string userId)
        {
            var url = $"https://{_auth0Domain}/api/v2/users/{userId}";
            var accessToken = await GetAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken)) throw new Exception("Не удалось получить токен");

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.DeleteAsync(url);

            if (!response.IsSuccessStatusCode) throw new Exception($"Ошибка удаления пользователя в Auth0: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
        }

        public string GetRoleId(string roleName)
        {
            var roles = new Dictionary<string, string>
            {
                { "Admin", "rol_6JJSMFRpkvMKUCr8" },
                { "Employee", "rol_1zXKaYdC0yYkur5R" },
                { "Manager", "rol_MroJU4loFATOSJTp" }
            };

            return roles.ContainsKey(roleName) ? roles[roleName] : null;
        }
    }

}
