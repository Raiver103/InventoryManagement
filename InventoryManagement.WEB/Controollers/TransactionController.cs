﻿using AutoMapper;
using InventoryManagement.Application.DTOs.Transaction;
using InventoryManagement.Application.Services;
using InventoryManagement.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Text;
using ClosedXML.Excel;

namespace InventoryManagement.WEB.Controollers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly TransactionService _transactionService;
        private readonly ItemService _itemService;
        private readonly ReportService _reportService;
        private readonly IMapper _mapper;
        private readonly IHubContext<InventoryHub> _hubContext;

        public TransactionController(
            TransactionService transactionService, 
            ItemService itemService, 
            IMapper mapper,
            IHubContext<InventoryHub> hubContext,
            ReportService reportService
            ) 
        {
            _transactionService = transactionService;
            _itemService = itemService;
            _mapper = mapper;
            _hubContext = hubContext;
            _reportService = reportService;
        }

        // Получение всех транзакций
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var transactions = await _transactionService.GetAllTransactions();
            var transactionsDto = _mapper.Map<IEnumerable<TransactionResponseDTO>>(transactions);
            return Ok(transactionsDto);
        }

        // Получение транзакции по ID
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var transaction = await _transactionService.GetTransactionById(id);
            if (transaction == null)
            {
                return NotFound();
            }
            var transactionDto = _mapper.Map<TransactionResponseDTO>(transaction);
            return Ok(transactionDto);
        }

        // Создание транзакции
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TransactionCreateDTO transactionCreateDto)
        {
            if (transactionCreateDto == null)
            {
                return BadRequest("Transaction data is required.");
            }

            // Проверка: нельзя переместить товар в ту же локацию
            if (transactionCreateDto.FromLocationId == transactionCreateDto.ToLocationId)
            {
                return BadRequest("Item cannot be moved to the same location.");
            }

            // Загружаем товар по ID
            var item = await _itemService.GetItemById(transactionCreateDto.ItemId);
            if (item == null)
            {
                return NotFound("Item not found.");
            }

            // Проверяем, что FromLocationId совпадает с текущим местоположением товара
            if (item.LocationId != transactionCreateDto.FromLocationId)
            {
                return BadRequest("Item's current location does not match FromLocationId.");
            }

            // Маппинг CreateDTO в сущность
            var transaction = _mapper.Map<Transaction>(transactionCreateDto);

            // Добавляем транзакцию
            await _transactionService.AddTransaction(transaction);


            // Маппинг сущности в ResponseDTO для ответа
            var createdTransactionDto = _mapper.Map<TransactionResponseDTO>(transaction);

            await _hubContext.Clients.All.SendAsync("ReceiveUpdate", createdTransactionDto);

            return CreatedAtAction(nameof(Get), new { id = transaction.Id }, createdTransactionDto);
        }

        [HttpGet("export/{format}")]
        public async Task<IActionResult> ExportTransactions(string format)
        {
            var transactions = await _transactionService.GetAllTransactions();
            var transactionsDto = _mapper.Map<IEnumerable<TransactionResponseDTO>>(transactions);

            if (format.ToLower() == "csv")
            {
                var csvData = _reportService.GenerateCsv(transactionsDto);
                return File(Encoding.UTF8.GetBytes(csvData), "text/csv", "transactions.csv");
            }
            else if (format.ToLower() == "excel")
            {
                var excelFile = _reportService.GenerateExcel(transactionsDto);
                return File(excelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "transactions.xlsx");
            }

            return BadRequest("Invalid format. Use 'csv' or 'excel'.");
        }
    }
}
