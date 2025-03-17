using InventoryManagement.Application.DTOs.Transaction;

namespace InventoryManagement.Application.Interfaces
{
    public interface IReportService
    {
        string GenerateCsv(IEnumerable<TransactionResponseDTO> transactions);
        byte[] GenerateExcel(IEnumerable<TransactionResponseDTO> transactions);
    }
}
