using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EcoShopApi.Application.Common.DTO.UserDTO
{
    public class LoginDto
    {
        [JsonPropertyName("username")]
        public required string UserName { get; set; }


        [JsonPropertyName("password")]
        public required string Password { get; set; }
    }
}

