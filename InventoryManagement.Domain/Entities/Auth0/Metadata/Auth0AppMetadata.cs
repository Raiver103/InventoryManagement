using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Domain.Entities.Auth0.Metadata
{
    public class Auth0AppMetadata
    {
        [JsonProperty("role")]
        public string Role { get; set; }
    }
}
