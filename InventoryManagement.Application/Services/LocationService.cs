using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Interfaces;

namespace InventoryManagement.Application.Services
{
    public class LocationService : ILocationService
    {
        private readonly ILocationRepository _locationRepository;

        public LocationService(ILocationRepository locationRepository)
        {
            _locationRepository = locationRepository;
        }

        public async Task<IEnumerable<Location>> GetAllLocations()
        {
            return await _locationRepository.GetAllAsync();
        }

        public async Task<Location> GetLocationById(int id)
        {
            return await _locationRepository.GetByIdAsync(id);
        }

        public async Task AddLocation(Location item)
        {
            await _locationRepository.AddAsync(item);
        }

        public async Task UpdateLocation(Location item)
        {
            await _locationRepository.UpdateAsync(item);
        }

        public async Task DeleteLocation(int id)
        {
            await _locationRepository.DeleteAsync(id);
        } 
    }
}
