using Microsoft.AspNetCore.Mvc;
using InventoryManagement.Domain.Entities.Auth0;
using InventoryManagement.Application.Interfaces;

[Route("api/auth0")]
[ApiController]
public class Auth0Controller : ControllerBase
{
    private readonly IAuth0Service _auth0Service;

    public Auth0Controller(IAuth0Service auth0Service)
    {
        _auth0Service = auth0Service;
    }

    [ApiExplorerSettings(IgnoreApi = true)] 
    public async Task<string> GetAccessTokenAsync()
    {
        return await _auth0Service.GetAccessTokenAsync();
    }

    [HttpPost("create-user")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var user = await _auth0Service.CreateUserAsync(request);
        return Ok(user);
    }

    [HttpPatch("update-user/{userId}")]
    public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserRequest request)
    {
        var user = await _auth0Service.UpdateUserAsync(userId, request);
        return Ok(user);
    }

    [HttpGet("get-users")]
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

    [HttpDelete("delete-user/{userId}")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        await _auth0Service.DeleteUserAsync(userId);
        return NoContent();
    }
}
