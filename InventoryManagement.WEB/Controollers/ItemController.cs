using AutoMapper;
using InventoryManagement.Application.DTOs.Item;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Application.Services;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Infastructure.Hubs; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace InventoryManagement.WEB.Controollers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemController : ControllerBase
    {
        private readonly IItemService _itemService;
        private readonly IMapper _mapper;
        private readonly IHubContext<InventoryHub> _hubContext;

        public ItemController(IItemService itemService, IMapper mapper, IHubContext<InventoryHub> hubContext)
        {
            _itemService = itemService;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        // Получение всех товаров
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _itemService.GetAllItems();
            var itemsDto = _mapper.Map<IEnumerable<ItemResponseDTO>>(items);
            return Ok(itemsDto);
        }

        // Получение товара по ID
        [HttpGet("{id}")] 
        public async Task<IActionResult> Get(int id)
        {
            var item = await _itemService.GetItemById(id);
            if (item == null)
            {
                return NotFound();
            }
            var itemDto = _mapper.Map<ItemResponseDTO>(item);
            return Ok(itemDto);
        }

        // Создание товара
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ItemCreateDTO itemCreateDto)
        {
            if (itemCreateDto == null)
            {
                return BadRequest();
            }

            // Маппинг DTO в Entity
            var item = _mapper.Map<Item>(itemCreateDto);
            await _itemService.AddItem(item);

            // Маппинг созданной сущности обратно в DTO для ответа
            var createdItemDto = _mapper.Map<ItemResponseDTO>(item);

            await _hubContext.Clients.All.SendAsync("ReceiveUpdate", createdItemDto);

            return CreatedAtAction(nameof(Get), new { id = item.Id }, createdItemDto);
        }

        // Обновление товара
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ItemCreateDTO itemUpdateDto)
        {
            if (itemUpdateDto == null)
            {
                return BadRequest();
            }

            var item = await _itemService.GetItemById(id);
            if (item == null)
            {
                return NotFound();
            }

            // Маппинг DTO в существующую сущность
            _mapper.Map(itemUpdateDto, item);
            await _itemService.UpdateItem(item);

            var updatedItemDto = _mapper.Map<ItemResponseDTO>(item);
            await _hubContext.Clients.All.SendAsync("ReceiveUpdate", updatedItemDto);

            return NoContent();
        }

        // Удаление товара
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _itemService.DeleteItem(id);
            await _hubContext.Clients.All.SendAsync("ItemDeleted", id);
            return NoContent();
        }
    }
}
