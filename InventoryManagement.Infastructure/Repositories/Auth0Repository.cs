using Microsoft.Extensions.Configuration;
using Newtonsoft.Json; 
using System.Text; 
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

            var responseString = await SendPostRequestAsync(url, payload);
            var tokenResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);

            return tokenResponse?.GetValueOrDefault("access_token")
                ?? throw new Exception("Ответ не содержит access_token");
        }

        public async Task<Auth0UserResponse> CreateUserAsync(CreateUserRequest request)
        {
            var url = $"https://{_auth0Domain}/api/v2/users";
            var accessToken = await GetAccessTokenAsync();
            var auth0Payload = new
            {
                email = request.Email,
                password = request.Password,
                connection = "Username-Password-Authentication",
                user_metadata = new { first_name = request.FirstName, last_name = request.LastName },
                app_metadata = new { role = request.Role }
            };

            var responseString = await SendPostRequestAsync(url, auth0Payload, accessToken);
            var createdUser = JsonConvert.DeserializeObject<Auth0UserResponse>(responseString);
            await AssignRoleToUserAsync(createdUser.Id, request.Role);

            return createdUser;
        }

        public async Task<Auth0UserResponse> UpdateUserAsync(string userId, UpdateUserRequest request)
        {
            var url = $"https://{_auth0Domain}/api/v2/users/{userId}";
            var accessToken = await GetAccessTokenAsync();
            var updatePayload = new Dictionary<string, object>
            {
                { "user_metadata", new { first_name = request.FirstName, last_name = request.LastName } },
                { "app_metadata", new { role = request.Role } }
            };

            if (!userId.Contains("google-oauth2"))
            {
                updatePayload["email"] = request.Email;
            }

            var responseString = await SendPatchRequestAsync(url, updatePayload, accessToken);
            await UpdateUserRolesAsync(userId, request.Role);

            return JsonConvert.DeserializeObject<Auth0UserResponse>(responseString);
        }

        public async Task<List<Auth0UserResponse>> GetUsersAsync()
        {
            var url = $"https://{_auth0Domain}/api/v2/users";
            var accessToken = await GetAccessTokenAsync();
            var responseString = await SendGetRequestAsync(url, accessToken);

            return JsonConvert.DeserializeObject<List<Auth0UserResponse>>(responseString);
        }

        public async Task DeleteUserAsync(string userId)
        {
            var url = $"https://{_auth0Domain}/api/v2/users/{userId}";
            var accessToken = await GetAccessTokenAsync();
            await SendDeleteRequestAsync(url, accessToken);
        }

        private async Task AssignRoleToUserAsync(string userId, string role)
        {
            var roleId = GetRoleId(role);
            if (string.IsNullOrEmpty(roleId))
                throw new Exception("Ошибка: Указанная роль не найдена");

            var url = $"https://{_auth0Domain}/api/v2/users/{userId}/roles";
            var payload = new { roles = new List<string> { roleId } };
            var accessToken = await GetAccessTokenAsync();

            await SendPostRequestAsync(url, payload, accessToken);
        }

        private async Task UpdateUserRolesAsync(string userId, string newRole)
        {
            var newRoleId = GetRoleId(newRole);
            if (string.IsNullOrEmpty(newRoleId))
                throw new Exception("Ошибка: Указанная роль не найдена");

            var url = $"https://{_auth0Domain}/api/v2/users/{userId}/roles";
            var accessToken = await GetAccessTokenAsync();
            var responseString = await SendGetRequestAsync(url, accessToken);

            var currentRoles = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(responseString);
            var currentRoleIds = currentRoles?.ConvertAll(r => r["id"]) ?? new List<string>();

            if (!currentRoleIds.Contains(newRoleId))
            {
                if (currentRoleIds.Count > 0)
                {
                    await SendDeleteRequestAsync(url, accessToken, new { roles = currentRoleIds });
                }
                await SendPostRequestAsync(url, new { roles = new List<string> { newRoleId } }, accessToken);
            }
        }

        private async Task<string> SendPostRequestAsync(string url, object payload, string accessToken = null)
        {
            return await SendHttpRequestAsync(HttpMethod.Post, url, payload, accessToken);
        }

        private async Task<string> SendGetRequestAsync(string url, string accessToken)
        {
            return await SendHttpRequestAsync(HttpMethod.Get, url, null, accessToken);
        }

        private async Task<string> SendPatchRequestAsync(string url, object payload, string accessToken)
        {
            return await SendHttpRequestAsync(HttpMethod.Patch, url, payload, accessToken);
        }

        private async Task SendDeleteRequestAsync(string url, string accessToken, object payload = null)
        {
            await SendHttpRequestAsync(HttpMethod.Delete, url, payload, accessToken);
        }

        private async Task<string> SendHttpRequestAsync(HttpMethod method, string url, object payload, string accessToken)
        {
            using var request = new HttpRequestMessage(method, url)
            {
                Content = payload != null ? new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json") : null
            };

            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            var response = await _httpClient.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Ошибка запроса: {response.StatusCode}, {responseString}");
            }

            return responseString;
        }

        public string GetRoleId(string roleName)
        {
            var rolesSection = _config.GetSection("Auth0Roles");
            var roles = rolesSection.GetChildren().ToDictionary(x => x.Key, x => x.Value);
            return roles.TryGetValue(roleName, out var roleId) ? roleId : null;
        }
    }
}
