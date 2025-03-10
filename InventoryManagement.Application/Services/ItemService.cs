using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Application.Services
{
    public class ItemService
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
            return await _itemRepository.GetByIdAsync(id);
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
