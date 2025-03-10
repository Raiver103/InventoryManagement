using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Application.DTOs.Location
{
    public class LocationCreateDTO
    {
        public string Name { get; set; }
        public string Address { get; set; }
    }
}
