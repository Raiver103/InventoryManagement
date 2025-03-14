using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Interfaces;
using System.Security.Claims;

namespace InventoryManagement.Application.Services
{
    public class AccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IAuth0Repository _auth0Repository;
        private readonly UserService _userService;

        public AccountService(IAccountRepository accountRepository, IAuth0Repository auth0Repository, UserService userService)
        {
            _accountRepository = accountRepository;
            _auth0Repository = auth0Repository;
            _userService = userService;
        }

        public async Task SyncUserAsync(ClaimsPrincipal user)
        {
            if (!user.Identity.IsAuthenticated) 
                return;

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) 
                return;

            var email = user.FindFirst(c => c.Type == "nickname")?.Value ?? "Не указано";
            var firstName = user.FindFirst("https://your-app.com/first_name")?.Value ?? "Не указано";
            var lastName = user.FindFirst("https://your-app.com/last_name")?.Value ?? "Не указано";
            var password = user.FindFirst(c => c.Type == "sid")?.Value ?? "Не указано";

            var existingUser = await _userService.GetUserById(userId);

            if (existingUser == null)
            {
                var newUser = new User
                {
                    Id = userId,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    Role = "Admin",
                    PasswordHash = password
                };

                await _userService.AddUser(newUser);
            }
        }

        public async Task AssignRoleAfterLoginAsync(ClaimsPrincipal user, string role)
        {
            if (!user.Identity.IsAuthenticated) return;

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new Exception("Ошибка: Не удалось получить идентификатор пользователя.");

            var existingRoles = await _accountRepository.GetUserRolesAsync(userId);
            if (existingRoles == null || !existingRoles.Any())
            {
                var roleId = _auth0Repository.GetRoleId(role);
                if (string.IsNullOrEmpty(roleId))
                    throw new Exception($"Ошибка: Роль {role} не найдена");

                await _accountRepository.AssignRoleAsync(userId, roleId);
            }
        }
    }

}
