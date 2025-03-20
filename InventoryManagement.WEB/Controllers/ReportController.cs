using InventoryManagement.Application.DTOs.Transaction;
using InventoryManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations; 
using System.Text;

namespace InventoryManagement.WEB.Controllers
{
    [ApiController]
    [Route("api/reports")]
    public class ReportController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly IReportService _reportService;

        public ReportController(ITransactionService transactionService, IReportService reportService)
        {
            _transactionService = transactionService;
            _reportService = reportService;
        }

        /// <summary>
        /// Экспортирует все транзакции в указанный формат.
        /// </summary>
        /// <param name="format">Формат файла: "csv" или "excel".</param>
        /// <returns>Файл с транзакциями.</returns>
        [HttpGet("export/{format}")]
        [SwaggerOperation(Summary = "Экспорт транзакций", Description = "Позволяет экспортировать все транзакции в CSV или Excel.")]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ExportTransactions(string format)
        {
            var transactions = await _transactionService.GetAllTransactions();
            var transactionsDto = transactions.Select(t => new TransactionResponseDTO
            {
                Id = t.Id,
                ItemId = t.ItemId,
                FromLocationId = t.FromLocationId,
                ToLocationId = t.ToLocationId,
                UserId = t.UserId,
                Timestamp = t.Timestamp
            });

            return format.ToLower() switch
            {
                "csv" => File(Encoding.UTF8.GetBytes(_reportService.GenerateCsv(transactionsDto)), "text/csv", "transactions.csv"),
                "excel" => File(_reportService.GenerateExcel(transactionsDto), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "transactions.xlsx"),
                _ => BadRequest("Invalid format. Use 'csv' or 'excel'.")
            };
        }
    }
}
