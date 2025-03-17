using InventoryManagement.Application.DTOs.Transaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Application.Interfaces
{
    public interface IReportService
    {
        /// <summary>
        /// Генерирует CSV-файл на основе списка транзакций.
        /// </summary>
        /// <param name="transactions">Список транзакций.</param>
        /// <returns>CSV-строка.</returns>
        string GenerateCsv(IEnumerable<TransactionResponseDTO> transactions);

        /// <summary>
        /// Генерирует Excel-файл на основе списка транзакций.
        /// </summary>
        /// <param name="transactions">Список транзакций.</param>
        /// <returns>Массив байтов, представляющий Excel-файл.</returns>
        byte[] GenerateExcel(IEnumerable<TransactionResponseDTO> transactions);
    }
}
