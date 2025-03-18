using AutoMapper;
using InventoryManagement.Application.DTOs.Item;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Infrastructure.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Swashbuckle.AspNetCore.Annotations; // Добавлено для Swagger

namespace InventoryManagement.WEB.Controllers
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

        /// <summary>
        /// Получает список всех товаров.
        /// </summary>
        /// <returns>Список товаров.</returns>
        [HttpGet]
        [SwaggerOperation(Summary = "Получить все товары", Description = "Возвращает список всех товаров.")]
        [ProducesResponseType(typeof(IEnumerable<ItemResponseDTO>), 200)]
        public async Task<IActionResult> GetAll()
        {
            var items = await _itemService.GetAllItems();
            var itemsDto = _mapper.Map<IEnumerable<ItemResponseDTO>>(items);
            return Ok(itemsDto);
        }

        /// <summary>
        /// Получает товар по ID.
        /// </summary>
        /// <param name="id">ID товара.</param>
        /// <returns>Информация о товаре.</returns>
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Получить товар по ID", Description = "Возвращает информацию о конкретном товаре.")]
        [ProducesResponseType(typeof(ItemResponseDTO), 200)]
        [ProducesResponseType(404)]
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

        /// <summary>
        /// Создает новый товар.
        /// </summary>
        /// <param name="itemCreateDto">Данные товара.</param>
        /// <returns>Созданный товар.</returns>
        [HttpPost]
        [SwaggerOperation(Summary = "Создать товар", Description = "Создает новый товар в системе.")]
        [ProducesResponseType(typeof(ItemResponseDTO), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] ItemCreateDTO itemCreateDto)
        {
            if (itemCreateDto == null)
            {
                return BadRequest();
            }

            var item = _mapper.Map<Item>(itemCreateDto);
            await _itemService.AddItem(item);

            var createdItemDto = _mapper.Map<ItemResponseDTO>(item);

            await _hubContext.Clients.All.SendAsync("ReceiveUpdate", createdItemDto);

            return CreatedAtAction(nameof(Get), new { id = item.Id }, createdItemDto);
        }

        /// <summary>
        /// Обновляет данные товара.
        /// </summary>
        /// <param name="id">ID товара.</param>
        /// <param name="itemUpdateDto">Обновленные данные.</param>
        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Обновить товар", Description = "Обновляет информацию о товаре.")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
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

            _mapper.Map(itemUpdateDto, item);
            await _itemService.UpdateItem(item);

            var updatedItemDto = _mapper.Map<ItemResponseDTO>(item);
            await _hubContext.Clients.All.SendAsync("ReceiveUpdate", updatedItemDto);

            return NoContent();
        }

        /// <summary>
        /// Удаляет товар.
        /// </summary>
        /// <param name="id">ID товара.</param>
        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Удалить товар", Description = "Удаляет товар из системы.")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            await _itemService.DeleteItem(id);
            await _hubContext.Clients.All.SendAsync("ItemDeleted", id);
            return NoContent();
        }
    }
}
