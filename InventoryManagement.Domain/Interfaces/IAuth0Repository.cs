using InventoryManagement.Domain.Entities.Auth0;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Domain.Interfaces
{
    public interface IAuth0Repository
    {
        Task<string> GetAccessTokenAsync();
        Task<Auth0UserResponse> CreateUserAsync(CreateUserRequest request);
        Task<Auth0UserResponse> UpdateUserAsync(string userId, UpdateUserRequest request);
        Task<List<Auth0UserResponse>> GetUsersAsync();
        Task DeleteUserAsync(string userId);
        string GetRoleId(string roleName);
    }

}
