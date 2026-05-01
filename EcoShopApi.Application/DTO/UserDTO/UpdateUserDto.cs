using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoShopApi.Application.DTO.UserDTO
{
    public class UpdateUserDto
    {
        public string DisplayName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string[] Roles { get; set; } 
        public bool IsActive { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
