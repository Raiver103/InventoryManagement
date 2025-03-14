using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Domain.Interfaces
{
    public interface IAccountRepository
    {
        Task<List<string>> AssignRoleAfterLoginAsync(string role);
        Task SyncUserDBAsync();
    }
}
