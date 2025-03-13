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
        Task<List<Auth0UserResponse>> GetUsersAsync();
        Task<Auth0UserResponse> CreateUserAsync(UserCreateDTO request);
        Task<bool> UpdateUserAsync(string userId, UpdateUserRequest request);
        Task<bool> DeleteUserAsync(string userId);
    }

}
