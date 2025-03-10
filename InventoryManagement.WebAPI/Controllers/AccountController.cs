using System.Security.Claims;
using Auth0.AspNetCore.Authentication;
using InventoryManagement.Application.DTOs.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.WebAPI.Controllers;

public class AccountController : Controller
{
    [HttpGet("/Auth/Login")]
    public async Task LoginUser(string returnUrl = "/")
    {
        Console.WriteLine(returnUrl);
        var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
        // Indicate here where Auth0 should redirect the user after a login.
        // Note that the resulting absolute Uri must be added to the
        // **Allowed Callback URLs** settings for the app.
        .WithRedirectUri(returnUrl)
        .Build();
         
    await HttpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
    }

    public async Task Signup(string returnUrl = "/")
    {
        var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
            .WithParameter("screen_hint", "signup")
            .WithRedirectUri(returnUrl)
        .Build();

        await HttpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
    }

    [Authorize]
    [HttpGet("/Auth/Logout")]
    public async Task<IActionResult> Logout()
    {
        var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
            // Indicate here where Auth0 should redirect the user after a logout.
            // Note that the resulting absolute Uri must be added to the
            // **Allowed Logout URLs** settings for the app.
            .WithRedirectUri(Url.Action("Index", "Home"))
            .Build();

        await HttpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("https://localhost:7024/");
    }

    [Authorize]
    [HttpGet("/Auth/Profile")]
    public IActionResult Profile()
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        // In your database, check if user exist's or not.
        // if(userNotExists)=> create new entry with 'userId'. You can alsow save Other user info.

        var userProfile = new UserCreateDTO
        {
            FirstName = User.Identity.Name,
            Email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
        };
        return View(userProfile);
    }

    public IActionResult EmailVerification()
    {
        return View();
    }

    public IActionResult AccessDenied()
    {
        return View();
    }
     

}