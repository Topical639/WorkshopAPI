using System.Text.Json.Serialization;

namespace WorkshopAPI.Models
{
    public class UserUpdateDto
    {
        [JsonPropertyName("user_name")]
        public string? Username { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

    }
}
