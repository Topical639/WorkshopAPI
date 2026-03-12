using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WorkshopAPI.Models
{
    public class RefreshTokenDto
    {
        [Required]
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;

    }
}
