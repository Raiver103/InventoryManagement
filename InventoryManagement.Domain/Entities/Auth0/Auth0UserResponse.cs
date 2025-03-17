using InventoryManagement.Domain.Entities.Auth0.Metadata;
using Newtonsoft.Json;

namespace InventoryManagement.Domain.Entities.Auth0
{
    public class Auth0UserResponse
    {
        [JsonProperty("user_id")]
        public string Id { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("user_metadata")]
        public Auth0UserMetadata Metadata { get; set; }

        [JsonProperty("app_metadata")]
        public Auth0AppMetadata AppMetadata { get; set; }

        public string FirstName => Metadata?.FirstName;
        public string LastName => Metadata?.LastName;
        public string Role => AppMetadata?.Role;
    }
}
