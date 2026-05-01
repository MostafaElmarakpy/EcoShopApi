using EcoShopApi.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace EcoShopApi.Application.Services.Interface;

/// <summary>
/// Product service interface with fully async contracts.
/// All methods are properly async to avoid thread pool starvation.
/// </summary>
public interface IProductService
{
    Task<IReadOnlyList<Product>> GetProductsAsync();

    /// <summary>
    /// Returns null if product not found (no exceptions for not found).
    /// </summary>
    Task<Product?> GetProductByIdAsync(int id);

    /// <summary>
    /// Creates a product with optional image upload.
    /// </summary>
    Task CreateProductAsync(Product productToCreate, IFormFile? files);

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    Task UpdateProductAsync(Product product);

    /// <summary>
    /// Deletes a product by ID.
    /// </summary>
    Task DeleteProductAsync(int id);
}
