using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Domain.Entities
{
    public class Transaction
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public Item Item { get; set; }
        public int FromLocationId { get; set; }
        public Location FromLocation { get; set; }
        public int ToLocationId { get; set; }
        public Location ToLocation { get; set; }
        public string UserId { get; set; }
        public User User { get; set; } 
        public DateTime Timestamp { get; set; }
    }
}
