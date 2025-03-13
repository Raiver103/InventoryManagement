using InventoryManagement.Application.Services;
using InventoryManagement.Domain.Entities.Auth0;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Interfaces;
using BCrypt.Net;

public class Auth0Service
{
    private readonly IAuth0Repository _auth0Repository;
    private readonly UserService _userService;

    public Auth0Service(IAuth0Repository auth0Repository, UserService userService)
    {
        _auth0Repository = auth0Repository;
        _userService = userService;
    }

    public async Task<List<User>> GetUsersAsync()
    {
        var auth0Users = await _auth0Repository.GetUsersAsync();
        return auth0Users.Select(u => new User
        {
            Id = u.Id,
            Email = u.Email,
            FirstName = u.Metadata?.FirstName ?? "Не указано",
            LastName = u.Metadata?.LastName ?? "Не указано",
            Role = u.AppMetadata?.Role ?? "Не указано"
        }).ToList();
    }

    public async Task<User> CreateUserAsync(UserCreateDTO request)
    {
        var auth0User = await _auth0Repository.CreateUserAsync(request);

        var newUser = new User
        {
            Id = auth0User.Id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = request.Role,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        await _userService.AddUser(newUser);
        return newUser;
    }

    public async Task<bool> UpdateUserAsync(string userId, UpdateUserRequest request)
    {
        return await _auth0Repository.UpdateUserAsync(userId, request);
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        return await _auth0Repository.DeleteUserAsync(userId);
    }
}
