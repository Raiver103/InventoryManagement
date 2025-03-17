using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Application.Interfaces
{
    public interface IAccountService
    {
        Task SyncUserAsync(ClaimsPrincipal user);
        Task AssignRoleAfterLoginAsync(ClaimsPrincipal user, string role);
    }
}
