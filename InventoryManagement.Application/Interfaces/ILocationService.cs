using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Application.Interfaces
{
    public interface ILocationService
    {
        Task<IEnumerable<Location>> GetAllLocations();
        Task<Location> GetLocationById(int id);
        Task AddLocation(Location location);
        Task UpdateLocation(Location location);
        Task DeleteLocation(int id);
    }
}
