using InventoryManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Application.Interfaces
{
    public interface ITransactionService
    {
        /// <summary>
        /// Получить все транзакции.
        /// </summary>
        Task<IEnumerable<Transaction>> GetAllTransactions();

        /// <summary>
        /// Получить транзакцию по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор транзакции.</param>
        Task<Transaction> GetTransactionById(int id);

        /// <summary>
        /// Добавить новую транзакцию.
        /// </summary>
        /// <param name="transaction">Транзакция для добавления.</param>
        /// <exception cref="Exception">Выбрасывается, если Item или Location не найдены.</exception>
        Task AddTransaction(Transaction transaction);
    }
}
