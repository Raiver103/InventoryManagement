using Newtonsoft.Json;

namespace InventoryManagement.Domain.Entities.Auth0.Metadata
{
    public class Auth0UserMetadata
    {
        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }
    }
}
