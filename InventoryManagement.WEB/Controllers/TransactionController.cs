using AutoMapper;
using InventoryManagement.Application.DTOs.Transaction;
using InventoryManagement.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Text;
using InventoryManagement.Infrastructure.Hubs;
using InventoryManagement.Application.Interfaces;
using Swashbuckle.AspNetCore.Annotations; 

namespace InventoryManagement.WEB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly IMapper _mapper;
        private readonly IHubContext<InventoryHub> _hubContext;

        public TransactionController(
            ITransactionService transactionService,
            IMapper mapper,
            IHubContext<InventoryHub> hubContext
        )
        {
            _transactionService = transactionService;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Получить список всех транзакций.
        /// </summary>
        /// <returns>Список транзакций.</returns>
        [HttpGet]
        [SwaggerOperation(Summary = "Получить все транзакции", Description = "Возвращает список всех транзакций.")]
        [ProducesResponseType(typeof(IEnumerable<TransactionResponseDTO>), 200)]
        public async Task<IActionResult> GetAll()
        {
            var transactions = await _transactionService.GetAllTransactions();
            return Ok(_mapper.Map<IEnumerable<TransactionResponseDTO>>(transactions));
        }

        /// <summary>
        /// Получить транзакцию по ID.
        /// </summary>
        /// <param name="id">ID транзакции.</param>
        /// <returns>Транзакция.</returns>
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Получить транзакцию", Description = "Возвращает транзакцию по указанному ID.")]
        [ProducesResponseType(typeof(TransactionResponseDTO), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var transaction = await _transactionService.GetTransactionById(id);
                return Ok(_mapper.Map<TransactionResponseDTO>(transaction));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Создать новую транзакцию.
        /// </summary>
        /// <param name="transactionCreateDto">Данные новой транзакции.</param>
        /// <returns>Созданная транзакция.</returns>
        [HttpPost]
        [SwaggerOperation(Summary = "Создать транзакцию", Description = "Создает новую транзакцию и отправляет обновление через SignalR.")]
        [ProducesResponseType(typeof(TransactionResponseDTO), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Create([FromBody] TransactionCreateDTO transactionCreateDto)
        {
            if (transactionCreateDto == null)
                return BadRequest("Transaction data is required.");

            try
            {
                var createdTransaction = await _transactionService.AddTransaction(transactionCreateDto);
                var createdTransactionDto = _mapper.Map<TransactionResponseDTO>(createdTransaction);

                await _hubContext.Clients.All.SendAsync("ReceiveUpdate", createdTransactionDto);
                return CreatedAtAction(nameof(Get), new { id = createdTransaction.Id }, createdTransactionDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal Server Error", details = ex.Message });
            }
        }
    }
}
