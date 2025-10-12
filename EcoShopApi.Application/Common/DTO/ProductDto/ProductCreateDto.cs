using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoShopApi.Application.Common.DTO.UserDTO
{
    public class ProductCreateDto
    {
        public string Name { get; set; }
        public string ProductCode { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }

        public string? ExistingImages { get; set; }
        public IFormFile? Files { get; set; }
    }
}
