using InventoryManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Application.Interfaces
{
    public interface ILocationService
    {
        /// <summary>
        /// Получить все местоположения.
        /// </summary>
        Task<IEnumerable<Location>> GetAllLocations();

        /// <summary>
        /// Получить местоположение по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор местоположения.</param>
        Task<Location> GetLocationById(int id);

        /// <summary>
        /// Добавить новое местоположение.
        /// </summary>
        /// <param name="location">Местоположение для добавления.</param>
        Task AddLocation(Location location);

        /// <summary>
        /// Обновить существующее местоположение.
        /// </summary>
        /// <param name="location">Местоположение для обновления.</param>
        Task UpdateLocation(Location location);

        /// <summary>
        /// Удалить местоположение по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор местоположения.</param>
        Task DeleteLocation(int id);
    }
}
