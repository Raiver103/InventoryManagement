using InventoryManagement.Domain.Entities.Auth0;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Interfaces;
using InventoryManagement.Application.Interfaces;

public class Auth0Service : IAuth0Service
{
    private readonly IAuth0Repository _auth0Repository;
    private readonly IUserRepository _userRepository;

    public Auth0Service(IAuth0Repository auth0Repository, IUserRepository userRepository)
    {
        _auth0Repository = auth0Repository;
        _userRepository = userRepository;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        return await _auth0Repository.GetAccessTokenAsync();
    }

    public async Task<User> CreateUserAsync(CreateUserRequest request)
    {
        var auth0User = await _auth0Repository.CreateUserAsync(request);

        if (auth0User == null)
        {
            throw new Exception("Ошибка при создании пользователя в Auth0");
        }

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

        await _userRepository.AddAsync(newUser);
        return newUser;
    }

    public async Task<User> UpdateUserAsync(string userId, UpdateUserRequest request)
    {
        var auth0User = await _auth0Repository.UpdateUserAsync(userId, request);

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) 
            throw new Exception("Пользователь не найден в базе данных");

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.Role = request.Role;

        await _userRepository.UpdateAsync(user);
        return user;
    }

    public async Task<List<Auth0UserResponse>> GetUsersAsync()
    {
        return await _auth0Repository.GetUsersAsync();
    }

    public async Task DeleteUserAsync(string userId)
    {
        try
        {
            await _userRepository.DeleteAsync(userId);  
            await _auth0Repository.DeleteUserAsync(userId);  
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка при удалении пользователя из БД: {ex.Message}", ex);
        }
    }
}
