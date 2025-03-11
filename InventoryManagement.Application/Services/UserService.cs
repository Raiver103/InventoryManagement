using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Application.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<User>> GetAllUsers()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<User> GetUserById(string id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task AddUser(User user)
        {
            await _userRepository.AddAsync(user);
        }

        public async Task UpdateUser(User user)
        {
            await _userRepository.UpdateAsync(user);
        }

        public async Task DeleteUser(string id)
        {
            await _userRepository.DeleteAsync(id);
        }

        public async Task<User> GetUserByEmail(string email) // Добавляем новый метод
        {
            return await _userRepository.GetByEmailAsync(email);
        }
    }
}
