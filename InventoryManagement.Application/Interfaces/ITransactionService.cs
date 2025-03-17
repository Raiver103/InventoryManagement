using InventoryManagement.Application.DTOs.Transaction;
using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Application.Interfaces
{
    public interface ITransactionService
    {
        Task<IEnumerable<Transaction>> GetAllTransactions();
        Task<Transaction> GetTransactionById(int id);
        Task<Transaction> AddTransaction(TransactionCreateDTO transaction);
    }
}
