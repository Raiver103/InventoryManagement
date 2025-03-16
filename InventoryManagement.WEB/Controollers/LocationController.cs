using AutoMapper;
using InventoryManagement.Application.DTOs.Location;
using InventoryManagement.Application.Services;
using InventoryManagement.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.WEB.Controollers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationController : ControllerBase
    {
        private readonly LocationService _locationService;
        private readonly IMapper _mapper;

        public LocationController(LocationService locationService, IMapper mapper)
        {
            _locationService = locationService;
            _mapper = mapper;
        }

        // Получение всех локаций
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var locations = await _locationService.GetAllLocation();
            var locationDtos = _mapper.Map<IEnumerable<LocationResponseDTO>>(locations);
            return Ok(locationDtos);
        }

        // Получение локации по ID
        [HttpGet("{id}")]
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

        // Создание новой локации
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Create([FromBody] LocationCreateDTO locationCreateDto)
        {
            if (locationCreateDto == null)
            {
                return BadRequest();
            }

            // Маппинг CreateDTO в сущность
            var location = _mapper.Map<Location>(locationCreateDto);
            await _locationService.AddLocation(location);

            // Маппинг сущности в ResponseDTO для ответа
            var createdLocationDto = _mapper.Map<LocationResponseDTO>(location);
            return CreatedAtAction(nameof(Get), new { id = location.Id }, createdLocationDto);
        }

        // Обновление локации
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] LocationCreateDTO locationUpdateDto)
        {
            if (locationUpdateDto == null)
            {
                return BadRequest();
            }

            // Получаем локацию по ID
            var location = await _locationService.GetLocationById(id);
            if (location == null)
            {
                return NotFound();
            }

            // Маппинг CreateDTO в существующую сущность
            _mapper.Map(locationUpdateDto, location);

            // Сохраняем изменения в базе данных
            await _locationService.UpdateLocation(location);

            // Возвращаем 204 (No Content) в случае успеха
            return NoContent();
        }

        // Удаление локации
        [HttpDelete("{id}")]
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
