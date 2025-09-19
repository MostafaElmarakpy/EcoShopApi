using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoShopApi.Application.Common.DTO.UserDTO
{
    public class UserDto
    {
        public required string DisplayName { get; set; }
        public required string Email { get; set; }
        public required string Token { get; set; }
        public string? RefreshToken { get; set; }
    }
}
