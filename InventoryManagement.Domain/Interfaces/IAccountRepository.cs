namespace InventoryManagement.Domain.Interfaces
{
    public interface IAccountRepository
    {
        Task<List<string>> GetUserRolesAsync(string userId);
        Task AssignRoleAsync(string userId, string roleId);
    }
}
