using Microsoft.AspNetCore.Mvc;
using InventoryManagement.Domain.Entities.Auth0;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Application.DTOs.User;
using Swashbuckle.AspNetCore.Annotations; // Добавлено для аннотаций Swagger

namespace InventoryManagement.WEB.Controllers
{
    [Route("api/auth0")]
    [ApiController]
    public class Auth0Controller : ControllerBase
    {
        private readonly IAuth0Service _auth0Service;

        public Auth0Controller(IAuth0Service auth0Service)
        {
            _auth0Service = auth0Service;
        }

        /// <summary>
        /// Получает токен доступа Auth0.
        /// </summary>
        /// <returns>Строка с access_token.</returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<string> GetAccessTokenAsync()
        {
            return await _auth0Service.GetAccessTokenAsync();
        }

        /// <summary>
        /// Создает нового пользователя в Auth0.
        /// </summary>
        /// <param name="request">Данные нового пользователя.</param>
        /// <returns>Созданный пользователь.</returns>
        [HttpPost("create-user")]
        [SwaggerOperation(Summary = "Создает пользователя", Description = "Создает нового пользователя в Auth0.")]
        [ProducesResponseType(typeof(UserResponseDTO), 200)]
        [ProducesResponseType(400, Type = typeof(object))]
        [ProducesResponseType(500, Type = typeof(object))]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                var user = await _auth0Service.CreateUserAsync(request);

                var userResponse = new UserResponseDTO
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role
                };

                return Ok(userResponse);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Обновляет данные пользователя Auth0.
        /// </summary>
        /// <param name="userId">ID пользователя.</param>
        /// <param name="request">Обновленные данные.</param>
        /// <returns>Обновленный пользователь.</returns>
        [HttpPatch("update-user/{userId}")]
        [SwaggerOperation(Summary = "Обновляет пользователя", Description = "Обновляет информацию о пользователе в Auth0.")]
        [ProducesResponseType(typeof(Auth0UserResponse), 200)]  
        [ProducesResponseType(400, Type = typeof(object))] 
        [ProducesResponseType(404, Type = typeof(object))] 
        [ProducesResponseType(500, Type = typeof(object))] 
        public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserRequest request)
        {
            var user = await _auth0Service.UpdateUserAsync(userId, request);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        /// <summary>
        /// Получает список всех пользователей из Auth0.
        /// </summary>
        /// <returns>Список пользователей.</returns>
        [HttpGet("get-users")]
        [SwaggerOperation(Summary = "Получает пользователей", Description = "Возвращает список пользователей из Auth0.")]
        [ProducesResponseType(typeof(IEnumerable<object>), 200)]
        public async Task<IActionResult> GetUsersFromAuth0()
        {
            var users = await _auth0Service.GetUsersAsync();
            var formattedUsers = users.Select(u => new
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.Metadata?.FirstName ?? "Не указано",
                LastName = u.Metadata?.LastName ?? "Не указано",
                Role = u.AppMetadata?.Role ?? "Не указано"
            });

            return Ok(formattedUsers);
        }

        /// <summary>
        /// Удаляет пользователя из Auth0.
        /// </summary>
        /// <param name="userId">ID пользователя.</param>
        [HttpDelete("delete-user/{userId}")]
        [SwaggerOperation(Summary = "Удаляет пользователя", Description = "Удаляет пользователя в Auth0.")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            await _auth0Service.DeleteUserAsync(userId);
            return NoContent();
        }
    }
}
