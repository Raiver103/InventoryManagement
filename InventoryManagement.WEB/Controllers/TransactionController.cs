using AutoMapper;
using InventoryManagement.Application.DTOs.Transaction;
using InventoryManagement.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Text;
using InventoryManagement.Infastructure.Hubs;
using InventoryManagement.Application.Interfaces;

namespace InventoryManagement.WEB.Controollers
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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var transactions = await _transactionService.GetAllTransactions();
            return Ok(_mapper.Map<IEnumerable<TransactionResponseDTO>>(transactions));
        }

        [HttpGet("{id}")]
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

        [HttpPost]
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
