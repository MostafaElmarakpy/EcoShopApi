using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EcoShopApi.Application.DTO.UserDTO
{
    public class ChangePasswordDto
    {
        [Required]
        [JsonPropertyName("currentPassword")]
        public string CurrentPassword { get; init; } = string.Empty;

        [Required]
        [JsonPropertyName("newPassword")]
        public string NewPassword { get; init; } = string.Empty;


    }
}
