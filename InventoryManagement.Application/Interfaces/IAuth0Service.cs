using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Entities.Auth0;

namespace InventoryManagement.Application.Interfaces
{
    public interface IAuth0Service
    {
        Task<string> GetAccessTokenAsync();
        Task<User> CreateUserAsync(CreateUserRequest request);
        Task<User> UpdateUserAsync(string userId, UpdateUserRequest request);
        Task<List<Auth0UserResponse>> GetUsersAsync();
        Task DeleteUserAsync(string userId);
    }
}
