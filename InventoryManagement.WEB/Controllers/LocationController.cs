using AutoMapper;
using InventoryManagement.Application.DTOs.Location;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations; 

namespace InventoryManagement.WEB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _locationService;
        private readonly IMapper _mapper;

        public LocationController(ILocationService locationService, IMapper mapper)
        {
            _locationService = locationService;
            _mapper = mapper;
        }

        /// <summary>
        /// Получает список всех локаций.
        /// </summary>
        /// <returns>Список локаций.</returns>
        [HttpGet]
        [SwaggerOperation(Summary = "Получить все локации", Description = "Возвращает список всех доступных локаций.")]
        [ProducesResponseType(typeof(IEnumerable<LocationResponseDTO>), 200)]
        public async Task<IActionResult> GetAll()
        {
            var locations = await _locationService.GetAllLocations();
            var locationDtos = _mapper.Map<IEnumerable<LocationResponseDTO>>(locations);
            return Ok(locationDtos);
        }

        /// <summary>
        /// Получает локацию по ID.
        /// </summary>
        /// <param name="id">ID локации.</param>
        /// <returns>Информация о локации.</returns>
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Получить локацию по ID", Description = "Возвращает данные конкретной локации.")]
        [ProducesResponseType(typeof(LocationResponseDTO), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Get(int id)
        {
            var location = await _locationService.GetLocationById(id);
            if (location == null)
            {
                return NotFound();
            }
            var locationDto = _mapper.Map<LocationResponseDTO>(location);
            return Ok(locationDto);
        }

        /// <summary>
        /// Создает новую локацию.
        /// </summary>
        /// <param name="locationCreateDto">Данные новой локации.</param>
        /// <returns>Созданная локация.</returns>
        [HttpPost]
        [IgnoreAntiforgeryToken]
        [SwaggerOperation(Summary = "Создать локацию", Description = "Создает новую локацию в системе.")]
        [ProducesResponseType(typeof(LocationResponseDTO), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] LocationCreateDTO locationCreateDto)
        {
            if (locationCreateDto == null)
            {
                return BadRequest();
            }

            var location = _mapper.Map<Location>(locationCreateDto);
            await _locationService.AddLocation(location);

            var createdLocationDto = _mapper.Map<LocationResponseDTO>(location);
            return CreatedAtAction(nameof(Get), new { id = location.Id }, createdLocationDto);
        }

        /// <summary>
        /// Обновляет данные локации.
        /// </summary>
        /// <param name="id">ID локации.</param>
        /// <param name="locationUpdateDto">Обновленные данные.</param>
        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Обновить локацию", Description = "Обновляет данные о локации.")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] LocationCreateDTO locationUpdateDto)
        {
            if (locationUpdateDto == null)
            {
                return BadRequest();
            }

            var location = await _locationService.GetLocationById(id);
            if (location == null)
            {
                return NotFound();
            }

            _mapper.Map(locationUpdateDto, location);
            await _locationService.UpdateLocation(location);

            return NoContent();
        }

        /// <summary>
        /// Удаляет локацию.
        /// </summary>
        /// <param name="id">ID локации.</param>
        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Удалить локацию", Description = "Удаляет локацию по заданному ID.")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            var location = await _locationService.GetLocationById(id);
            if (location == null)
            {
                return NotFound();
            }

            await _locationService.DeleteLocation(id);
            return NoContent();
        }
    }
}
