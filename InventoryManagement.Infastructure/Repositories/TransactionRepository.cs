using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Interfaces;
using InventoryManagement.Infastructure.Persistence;
using Microsoft.EntityFrameworkCore; 

namespace InventoryManagement.Infastructure.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext _context;

        public TransactionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Transaction transaction)
        {
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Transaction>> GetAllAsync()
        {
            return await _context.Transactions
                .Include(t => t.Item)
                .Include(t => t.FromLocation)
                .Include(t => t.ToLocation)
                .Include(t => t.User)
                .ToListAsync();
        }

        public async Task<Transaction> GetByIdAsync(int id)
        {
            return await _context.Transactions
                .Include(t => t.Item)
                .Include(t => t.FromLocation)
                .Include(t => t.ToLocation)
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);
        }
    }
}
