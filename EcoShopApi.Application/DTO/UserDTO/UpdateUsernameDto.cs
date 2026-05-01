using System.ComponentModel.DataAnnotations;

namespace EcoShopApi.Application.DTO.UserDTO
{
    /// <summary>
    /// DTO for changing username
    /// </summary>
    public class UpdateUsernameDto
    {
        [Required(ErrorMessage = "New username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Username can only contain letters, numbers, underscores, and hyphens")]
        public string NewUserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Current password is required")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;
    }
}
