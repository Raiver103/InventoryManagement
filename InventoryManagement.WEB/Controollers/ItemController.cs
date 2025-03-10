using AutoMapper;
using InventoryManagement.Application.DTOs.Item;
using InventoryManagement.Application.Services;
using InventoryManagement.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.WEB.Controollers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemController : ControllerBase
    {
        private readonly ItemService _itemService;
        private readonly IMapper _mapper;

        public ItemController(ItemService itemService, IMapper mapper)
        {
            _itemService = itemService;
            _mapper = mapper;
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
        [Authorize(Policy = "Admin")]
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

            return NoContent();
        }

        // Удаление товара
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _itemService.DeleteItem(id);
            return NoContent();
        }
    }
}
