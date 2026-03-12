using System.Text.Json.Serialization;

namespace WorkshopAPI.Models
{
    public class UserResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("user_name")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
