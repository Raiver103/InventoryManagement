using InventoryManagement.Application.DTOs.Transaction;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Interfaces;

namespace InventoryManagement.Application.Services
{
    public class TransactionService : ITransactionService
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

        public TransactionService()
        {
        }

        public async Task<IEnumerable<Transaction>> GetAllTransactions()
        {
            return await _transactionRepository.GetAllAsync();
        }

        public async Task<Transaction> GetTransactionById(int id)
        {
            return await _transactionRepository.GetByIdAsync(id);
        }

        public async Task<Transaction> AddTransaction(TransactionCreateDTO transactionCreateDto)
        {
            var item = await _itemRepository.GetByIdAsync(transactionCreateDto.ItemId);
            if (item == null)
                throw new ArgumentException("Item not found."); // ⚠️ Меняем на ArgumentException

            if (item.LocationId != transactionCreateDto.FromLocationId)
                throw new ArgumentException("Item's current location does not match FromLocationId.");

            if (transactionCreateDto.FromLocationId == transactionCreateDto.ToLocationId)
                throw new ArgumentException("Item cannot be moved to the same location."); // ⚠️ Тут тоже

            var fromLocation = await _locationRepository.GetByIdAsync(transactionCreateDto.FromLocationId);
            var toLocation = await _locationRepository.GetByIdAsync(transactionCreateDto.ToLocationId);
            if (fromLocation == null || toLocation == null)
                throw new ArgumentException("Invalid locations.");

            var transaction = new Transaction
            {
                ItemId = transactionCreateDto.ItemId,
                FromLocationId = transactionCreateDto.FromLocationId,
                ToLocationId = transactionCreateDto.ToLocationId,
                UserId = transactionCreateDto.UserId,
                Timestamp = DateTime.UtcNow
            };

            item.LocationId = transactionCreateDto.ToLocationId;
            await _itemRepository.UpdateAsync(item);
            await _transactionRepository.AddAsync(transaction);

            return transaction;
        }

    }
}
