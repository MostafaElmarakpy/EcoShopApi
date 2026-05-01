using System.ComponentModel.DataAnnotations.Schema;

namespace EcoShopApi.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string ProductCode { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? ImagePath { get; set; }
    public decimal Price { get; set; }
    public int MinimumQuantity { get; set; }
    public double DiscountRate { get; set; }
    public int CategoryId { get; set; }

    // Navigation property 
    public virtual Category? Category { get; set; }
}
