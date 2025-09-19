using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoShopApi.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string ProductCode { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? ImagePath { get; set; }
        public decimal Price { get; set; }
        public int MinimumQuantity { get; set; }
        public double DiscountRate { get; set; }
        [NotMapped]
        public IFormFile? Image { get; set; }
        public int CategoryId { get; set; }
        // Navigation property 
        public virtual Category? Category { get; set; }
    }
}
