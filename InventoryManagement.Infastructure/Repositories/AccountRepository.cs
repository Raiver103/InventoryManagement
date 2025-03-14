using InventoryManagement.Domain.Interfaces;
using InventoryManagement.Infastructure.Models.Auth0;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Infrastructure.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly string _auth0Domain;
        private readonly IAuth0Repository _auth0Repository;

        public AccountRepository(HttpClient httpClient, IConfiguration config, IAuth0Repository auth0Repository)
        {
            _httpClient = httpClient;
            _config = config;
            _auth0Domain = _config["Auth0:Domain"];
            _auth0Repository = auth0Repository;
        }

        public async Task<List<string>> GetUserRolesAsync(string userId)
        {
            var accessToken = await _auth0Repository.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken)) 
                throw new Exception("Не удалось получить токен");

            var rolesUrl = $"https://{_auth0Domain}/api/v2/users/{userId}/roles";

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.GetAsync(rolesUrl);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Ошибка получения ролей пользователя: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
            

            var responseString = await response.Content.ReadAsStringAsync();
            var roles = JsonConvert.DeserializeObject<List<Auth0Role>>(responseString);

            return roles?.Select(r => r.Name).ToList() ?? new List<string>();
        }

        public async Task AssignRoleAsync(string userId, string roleId)
        {
            var accessToken = await _auth0Repository.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken)) 
                throw new Exception("Не удалось получить токен");

            var roleAssignUrl = $"https://{_auth0Domain}/api/v2/users/{userId}/roles";
            var roleAssignPayload = new { roles = new List<string> { roleId } };
            var roleContent = new StringContent(JsonConvert.SerializeObject(roleAssignPayload), Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            var roleResponse = await _httpClient.PostAsync(roleAssignUrl, roleContent);

            if (!roleResponse.IsSuccessStatusCode)
                throw new Exception($"Ошибка назначения роли: {roleResponse.StatusCode}, {await roleResponse.Content.ReadAsStringAsync()}");
        }
    }
}
