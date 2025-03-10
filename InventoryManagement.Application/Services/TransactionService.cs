using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        // Добавить транзакцию и обновить местоположение товара
        public async Task AddTransaction(Transaction transaction)
        {
            // Сначала добавляем транзакцию
            await _transactionRepository.AddAsync(transaction);

            // Получаем товар по его ID
            var item = await _itemRepository.GetByIdAsync(transaction.ItemId);
            if (item == null)
            {
                throw new Exception("Item not found.");
            }

            // Обновляем местоположение товара
            item.LocationId = transaction.ToLocationId; // Изменяем местоположение товара на новое
            await _itemRepository.UpdateAsync(item); // Сохраняем изменения

            var fromLocation = await _locationRepository.GetByIdAsync(transaction.FromLocationId);
            var toLocation = await _locationRepository.GetByIdAsync(transaction.ToLocationId);
            if (fromLocation == null || toLocation == null)
            {
                throw new Exception("Location not found.");
            }
        }
    }
}
