using AutoMapper;
using InventoryManagement.Application.DTOs.User;
using InventoryManagement.Application.Services;
using InventoryManagement.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InventoryManagement.WEB.Controollers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly IMapper _mapper;

        public UserController(UserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        // Получение всех пользователей
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllUsers();
            var userDtos = _mapper.Map<IEnumerable<UserResponseDTO>>(users);
            return Ok(userDtos);
        }

        // Получение пользователя по ID
        [HttpGet("{id}")]
        public IActionResult GetUser()
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            var user = new
            {
                Name = User.FindFirst(ClaimTypes.Name)?.Value,
                Email = User.FindFirst(ClaimTypes.Email)?.Value
            };

            return Ok(user);
        }

        // Создание нового пользователя
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserCreateDTO userCreateDto)
        {
            if (userCreateDto == null)
            {
                return BadRequest();
            }

            // Маппинг CreateDTO в сущность
            var user = _mapper.Map<User>(userCreateDto);
            user.PasswordHash = userCreateDto.PasswordHash; // Хэшируем пароль

            await _userService.AddUser(user);

            // Маппинг сущности в ResponseDTO для ответа
            var createdUserDto = _mapper.Map<UserResponseDTO>(user);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, createdUserDto);
        }

        // Обновление пользователя
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UserCreateDTO userUpdateDto)
        {
            if (userUpdateDto == null)
            {
                return BadRequest();
            }

            // Получаем пользователя по ID
            var user = await _userService.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }

            // Маппинг CreateDTO в существующую сущность
            _mapper.Map(userUpdateDto, user);
            user.PasswordHash = userUpdateDto.PasswordHash; // Хэшируем пароль

            // Сохраняем изменения в базе данных
            await _userService.UpdateUser(user);

            // Возвращаем 204 (No Content) в случае успеха
            return NoContent();
        }

        // Удаление пользователя
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userService.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }

            await _userService.DeleteUser(id);
            return NoContent();
        }

        [Authorize]
        [HttpPost("sync-user")]
        public async Task<IActionResult> SyncUser()
        {
            const string name = "https://your-namespace/";
            var userEmail = User.FindFirst(name + "email")?.Value;

            if (string.IsNullOrEmpty(userEmail))
            {
                return BadRequest("Email not found in token.");
            }

            // Проверяем, существует ли пользователь с таким email
            var user = await _userService.GetUserByEmail(userEmail);

            if (user == null)
            {
                // Создаем нового пользователя
                user = new User
                {
                    Email = userEmail,
                    Role = "Employee", // Роль по умолчанию
                    FirstName = User.FindFirst(name + "given_name")?.Value,
                    LastName = User.FindFirst(name + "family_name")?.Value
                };

                // Добавляем пользователя в базу данных
                await _userService.AddUser(user);
            }

            return Ok(user);
        }
    }
}
