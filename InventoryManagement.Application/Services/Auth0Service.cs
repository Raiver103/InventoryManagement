using InventoryManagement.Application.Services;
using InventoryManagement.Domain.Entities.Auth0;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Interfaces;
using BCrypt.Net;
using Microsoft.Extensions.Configuration;

public class Auth0Service
{
    private readonly IAuth0Repository _auth0Repository;
    private readonly UserService _userService; 

    public Auth0Service(IAuth0Repository auth0Repository, UserService userService)
    {
        _auth0Repository = auth0Repository;
        _userService = userService;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        return await _auth0Repository.GetAccessTokenAsync();
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
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            ManagedItems = new List<Item>()
        };

        await _userService.AddUser(newUser);
        return newUser;
    }

    public async Task<User> UpdateUserAsync(string userId, UpdateUserRequest request)
    {
        var auth0User = await _auth0Repository.UpdateUserAsync(userId, request);

        var user = await _userService.GetUserById(userId);
        if (user == null) throw new Exception("Пользователь не найден в базе данных");

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.Role = request.Role;

        await _userService.UpdateUser(user);
        return user;
    }

    public async Task<List<Auth0UserResponse>> GetUsersAsync()
    {
        return await _auth0Repository.GetUsersAsync();
    }

    public async Task DeleteUserAsync(string userId)
    {
        await _auth0Repository.DeleteUserAsync(userId);
        await _userService.DeleteUser(userId);
    }
}
