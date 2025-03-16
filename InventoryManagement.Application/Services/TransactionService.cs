using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Interfaces;

namespace InventoryManagement.Application.Services
{
    public class TransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IItemRepository _itemRepository;
        private readonly ILocationRepository _locationRepository;

        public TransactionService(
            ITransactionRepository transactionRepository,
            IItemRepository itemRepository,
            ILocationRepository locationRepository)
        {
            _transactionRepository = transactionRepository;
            _itemRepository = itemRepository;
            _locationRepository = locationRepository;
        }

        public async Task<IEnumerable<Transaction>> GetAllTransactions()
        {
            return await _transactionRepository.GetAllAsync();
        }

        public async Task<Transaction> GetTransactionById(int id)
        {
            return await _transactionRepository.GetByIdAsync(id);
        }

        public async Task AddTransaction(Transaction transaction)
        {
            var item = await _itemRepository.GetByIdAsync(transaction.ItemId);
            if (item == null)
            {
                throw new Exception("Item not found."); 
            }

            item.LocationId = transaction.ToLocationId;
            await _itemRepository.UpdateAsync(item);

            var fromLocation = await _locationRepository.GetByIdAsync(transaction.FromLocationId);
            var toLocation = await _locationRepository.GetByIdAsync(transaction.ToLocationId);
            if (fromLocation == null || toLocation == null)
            {
                throw new Exception("Location not found."); 
            }

            await _transactionRepository.AddAsync(transaction);
        }

    }
}
