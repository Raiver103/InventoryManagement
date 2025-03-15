using InventoryManagement.Domain.Interfaces;
using InventoryManagement.Infastructure.Models.Auth0;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json; 
using System.Net.Http.Headers;
using System.Text; 

namespace InventoryManagement.Infrastructure.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly HttpClient _httpClient;
        private readonly string _auth0Domain;
        private readonly IAuth0Repository _auth0Repository;

        private const string Auth0ApiBase = "/api/v2/users/";

        public AccountRepository(HttpClient httpClient, IConfiguration config, IAuth0Repository auth0Repository)
        {
            _httpClient = httpClient;
            _auth0Domain = config["Auth0:Domain"] ?? throw new ArgumentNullException(nameof(config));
            _auth0Repository = auth0Repository;
        }

        public async Task<List<string>> GetUserRolesAsync(string userId)
        {
            var accessToken = await GetAccessTokenAsync();
            SetAuthorizationHeader(accessToken);

            var rolesUrl = $"https://{_auth0Domain}{Auth0ApiBase}{userId}/roles";
            var response = await _httpClient.GetAsync(rolesUrl);

            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var roles = JsonConvert.DeserializeObject<List<Auth0Role>>(responseString);

            return roles?.Select(r => r.Name).ToList() ?? new List<string>();
        }

        public async Task AssignRoleAsync(string userId, string roleId)
        {
            var accessToken = await GetAccessTokenAsync();
            SetAuthorizationHeader(accessToken);

            var roleAssignUrl = $"https://{_auth0Domain}{Auth0ApiBase}{userId}/roles";
            var roleAssignPayload = new { roles = new[] { roleId } };
            var roleContent = CreateJsonContent(roleAssignPayload);

            var response = await _httpClient.PostAsync(roleAssignUrl, roleContent);
            response.EnsureSuccessStatusCode();
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var accessToken = await _auth0Repository.GetAccessTokenAsync();
            return string.IsNullOrEmpty(accessToken)
                ? throw new InvalidOperationException("Не удалось получить токен")
                : accessToken;
        }

        private void SetAuthorizationHeader(string accessToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        private static StringContent CreateJsonContent(object payload)
        {
            return new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
        }
    }
}
