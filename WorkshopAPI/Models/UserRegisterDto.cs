using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WorkshopAPI.Models
{
    public class UserRegisterDto
    {
        [Required]
        [JsonPropertyName("user_name")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }
}
