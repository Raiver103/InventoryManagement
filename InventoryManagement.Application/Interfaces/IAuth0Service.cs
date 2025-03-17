using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Entities.Auth0;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
