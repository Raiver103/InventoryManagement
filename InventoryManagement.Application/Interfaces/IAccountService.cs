using System.Security.Claims;

namespace InventoryManagement.Application.Interfaces
{
    public interface IAccountService
    {
        Task SyncUserAsync(ClaimsPrincipal user);
        Task AssignRoleAfterLoginAsync(ClaimsPrincipal user, string role);
    }
}
