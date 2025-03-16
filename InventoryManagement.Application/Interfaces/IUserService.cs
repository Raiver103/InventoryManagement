using InventoryManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
