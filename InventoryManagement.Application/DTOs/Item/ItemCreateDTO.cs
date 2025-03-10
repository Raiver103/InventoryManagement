using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace InventoryManagement.Application.DTOs.Item
{
    public class ItemCreateDTO
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public int Quantity { get; set; }
        public int LocationId { get; set; }
    }
}
