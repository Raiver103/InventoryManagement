using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Application.Interfaces
{
    public interface IItemService
    {
        Task<IEnumerable<Item>> GetAllItems();
        Task<Item> GetItemById(int id);
        Task AddItem(Item item);
        Task UpdateItem(Item item);
        Task DeleteItem(int id);
    }
}
