using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Application.Interfaces
{
    public interface IUserService
    {
        Task<User> GetUserById(string userId);
        Task AddUser(User user);
        Task UpdateUser(User user);
        Task DeleteUser(string userId);
    }
}
