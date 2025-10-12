using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoShopApi.Application.Common.DTO.UserDTO
{
    public class UserAppDto
    {
        public string Id { get; set; } = null!;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = null!;
        public bool IsActive { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}
