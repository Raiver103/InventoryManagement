using InventoryManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Application.Interfaces
{
    public interface IItemService
    {
        /// <summary>
        /// Получить все элементы.
        /// </summary>
        Task<IEnumerable<Item>> GetAllItems();

        /// <summary>
        /// Получить элемент по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор элемента.</param>
        Task<Item> GetItemById(int id);

        /// <summary>
        /// Добавить новый элемент.
        /// </summary>
        /// <param name="item">Элемент для добавления.</param>
        Task AddItem(Item item);

        /// <summary>
        /// Обновить существующий элемент.
        /// </summary>
        /// <param name="item">Элемент для обновления.</param>
        Task UpdateItem(Item item);

        /// <summary>
        /// Удалить элемент по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор элемента.</param>
        Task DeleteItem(int id);
    }
}
