using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoShopApi.Application.Common.DTO.UserDTO
{
    public class RegisterDto
    {

        public required string UserName { get; set; }
        [EmailAddress]
        public required string Email { get; set; }
        [Required]

        public required string Password { get; set; }
        public required string ConfirmPassword { get; set; }
    }
}
