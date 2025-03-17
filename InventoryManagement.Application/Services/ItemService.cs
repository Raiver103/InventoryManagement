using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Interfaces;

namespace InventoryManagement.Application.Services
{
    public class ItemService : IItemService
    {
        private readonly IItemRepository _itemRepository;

        public ItemService(IItemRepository itemRepository) 
        { 
            _itemRepository = itemRepository; 
        }

        public async Task<IEnumerable<Item>> GetAllItems()
        {
            return await _itemRepository.GetAllAsync();
        }

        public async Task<Item> GetItemById(int id)
        {
            return await _itemRepository.GetByIdAsync(id) 
                ?? throw new KeyNotFoundException($"Item с ID {id} не найден.");
        }

        public async Task AddItem(Item item)
        {
            await _itemRepository.AddAsync(item);
        }

        public async Task UpdateItem(Item item)
        {
            await _itemRepository.UpdateAsync(item);
        }

        public async Task DeleteItem(int id)
        {
            await _itemRepository.DeleteAsync(id);
        }
    }
}
