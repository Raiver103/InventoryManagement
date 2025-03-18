using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using InventoryManagement.Application.DTOs.User;
using InventoryManagement.Application.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace InventoryManagement.WEB.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("[controller]")]
    [Produces("application/json")]
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        /// <summary>
        /// Обработчик после входа пользователя.
        /// </summary>
        /// <param name="returnUrl">URL перенаправления после входа</param>
        /// <returns>Перенаправление на страницу после входа</returns>
        [HttpGet("AfterLogin")]
        [SwaggerOperation(Summary = "Обработчик после входа пользователя", Description = "Назначает роль пользователю и синхронизирует данные")]
        public async Task<IActionResult> AfterLogin(string returnUrl = "/")
        {
            await _accountService.AssignRoleAfterLoginAsync(User, "Admin");
            await _accountService.SyncUserAsync(User);
            return Redirect("http://localhost:5000/afterlogin");
        }

        /// <summary>
        /// Перенаправляет пользователя на страницу входа.
        /// </summary>
        /// <param name="returnUrl">URL перенаправления после входа</param>
        /// <returns>Перенаправление на страницу входа</returns>
        [HttpGet("Login")]
        [SwaggerOperation(Summary = "Перенаправляет на страницу входа")]
        public async Task<IActionResult> Login(string returnUrl = "/")
        {
            return Redirect("https://localhost:7025/login");
        }

        /// <summary>
        /// Перенаправляет на страницу входа при отказе в доступе.
        /// </summary>
        /// <param name="returnUrl">URL перенаправления</param>
        /// <returns>Перенаправление на страницу входа</returns>
        [HttpGet("AccessDenied")]
        [SwaggerOperation(Summary = "Перенаправляет на страницу входа при отказе в доступе")]
        public IActionResult LoginUserRedirectAccessDenied(string returnUrl = "/")
        {
            return Redirect("https://localhost:7025/login");
        }

        /// <summary>
        /// Выполняет вход пользователя через Auth0.
        /// </summary>
        /// <param name="returnUrl">URL после входа</param>
        [HttpGet("Auth/Login")]
        [SwaggerOperation(Summary = "Вход пользователя через Auth0")]
        public async Task LoginUser(string returnUrl = "/Account/AfterLogin")
        {
            var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
                .WithRedirectUri(returnUrl)
                .Build();

            await HttpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
        }

        /// <summary>
        /// Выполняет регистрацию пользователя через Auth0.
        /// </summary>
        /// <param name="returnUrl">URL после регистрации</param>
        [HttpGet("Signup")]
        [SwaggerOperation(Summary = "Регистрация пользователя через Auth0")]
        public async Task Signup(string returnUrl = "/")
        {
            var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
                .WithParameter("screen_hint", "signup")
                .WithRedirectUri(returnUrl)
                .Build();

            await HttpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
        }

        /// <summary>
        /// Выполняет выход пользователя.
        /// </summary> 
        [HttpGet("Logout")]
        [SwaggerOperation(Summary = "Выход пользователя из системы")]
        public async Task Logout()
        {
            var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
                .WithRedirectUri(Url.Action("Index", "Home"))
                .Build();

            await HttpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Получает профиль текущего пользователя.
        /// </summary>
        /// <returns>Профиль пользователя</returns> 
        [HttpGet("Profile")]
        [SwaggerOperation(Summary = "Получить профиль текущего пользователя")]
        [ProducesResponseType(typeof(UserProfileDto), 200)]
        public async Task<IActionResult> Profile()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var userProfile = new UserProfileDto
            {
                FirstName = User.FindFirst("https://your-app.com/first_name")?.Value,
                LastName = User.FindFirst("https://your-app.com/last_name")?.Value,
                EmailAddress = User.FindFirst(c => c.Type == "nickname")?.Value,
                ProfileImage = User.Claims.FirstOrDefault(c => c.Type == "picture")?.Value
            };

            return View(userProfile);
        }
    }
}
