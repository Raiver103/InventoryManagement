using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using InventoryManagement.Application.DTOs.User;
using InventoryManagement.Application.Interfaces;

namespace InventoryManagement.WEB.Controollers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("/Account/AfterLogin")]
        public async Task<IActionResult> AfterLogin(string returnUrl = "/")
        {
            await _accountService.AssignRoleAfterLoginAsync(User, "Admin");
            await _accountService.SyncUserAsync(User);
            return Redirect("https://localhost:7025/afterlogin");
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("/Account/Login")]
        public async Task<IActionResult> Login(string returnUrl = "/")
        { 
            return Redirect("https://localhost:7025/login");
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("/Account/AccessDenied")]
        public IActionResult LoginUserRedirectAccessDenied(string returnUrl = "/")
        {
            return Redirect("https://localhost:7025/login");
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("/Auth/Login")]
        public async Task LoginUser(string returnUrl = "/Account/AfterLogin")
        { 
            var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
                .WithRedirectUri(returnUrl)
                .Build();

            await HttpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("/Account/Signup")]
        public async Task Signup(string returnUrl = "/")
        {
            var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
                .WithParameter("screen_hint", "signup")
                .WithRedirectUri(returnUrl)
            .Build();

            await HttpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize]
        [HttpGet("/Account/Logout")]
        public async Task Logout()
        {
            var authenticationProperties = new LogoutAuthenticationPropertiesBuilder() 
                .WithRedirectUri(Url.Action("Index", "Home"))
                .Build();

            await HttpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); 

        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize]
        [HttpGet("/Account/Profile")]
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
