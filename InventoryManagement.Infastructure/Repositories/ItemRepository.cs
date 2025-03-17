using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Interfaces;
using InventoryManagement.Infastructure.Persistence;
using Microsoft.EntityFrameworkCore; 

namespace InventoryManagement.Infastructure.Repositories
{
    public class ItemRepository : IItemRepository
    {
        private readonly AppDbContext _context;

        public ItemRepository(AppDbContext context) 
        { 
            _context = context; 
        }

        public async Task<IEnumerable<Item>> GetAllAsync()
        {
            return await _context.Items.ToListAsync();
        }

        public async Task<Item> GetByIdAsync(int id)
        {
            return await _context.Items.FindAsync(id);
        }

        public async Task AddAsync(Item item)
        {
            _context.Items.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Item item)
        {
            _context.Items.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var item = await GetByIdAsync(id) ?? throw new KeyNotFoundException($"Item с ID {id} не найден.");
            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
        }
    }
}
