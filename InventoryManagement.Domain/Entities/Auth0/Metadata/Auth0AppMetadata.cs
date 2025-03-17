using Newtonsoft.Json;

namespace InventoryManagement.Domain.Entities.Auth0.Metadata
{
    public class Auth0AppMetadata
    {
        [JsonProperty("role")]
        public string Role { get; set; }
    }
}
