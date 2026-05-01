using EcoShopApi.Application.Interfaces;
using EcoShopApi.Application.Services.Interface;
using EcoShopApi.Domain.Entities;
using EcoShopApi.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace EcoShopApi.Application.Services.Implementation;

/// <summary>
/// Product service with corrected async patterns and better error handling.
/// </summary>
public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductService> _logger;
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

    public ProductService(IUnitOfWork unitOfWork, ILogger<ProductService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Gets a product by ID. Returns null if not found (no exception).
    /// </summary>
    public async Task<Product?> GetProductByIdAsync(int id)
    {
        try
        {
            var product = await _unitOfWork.Product.Get(p => p.Id == id);
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product with ID {ProductId}", id);
            throw;
        }
    }

    /// <summary>
    /// Creates a product with optional image upload.
    /// Now fully async - no .Result blocking calls.
    /// </summary>
    public async Task CreateProductAsync(Product productToCreate, IFormFile? files)
    {
        try
        {
            // Save product first
            await _unitOfWork.Product.Add(productToCreate);
            await _unitOfWork.Save();

            // Then save image if provided
            if (files != null)
            {
                var imagePath = await SaveProductImageAsync(files); // ✅ Proper await
                if (!string.IsNullOrEmpty(imagePath))
                {
                    productToCreate.ImagePath = imagePath;
                    _unitOfWork.Product.Update(productToCreate);
                    await _unitOfWork.Save();
                }
            }

            _logger.LogInformation("Product created: {ProductId}", productToCreate.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            throw;
        }
    }

    /// <summary>
    /// Updates an existing product with new image if provided.
    /// </summary>
    public async Task UpdateProductAsync(Product product)
    {
        try
        {
            var existingProduct = await _unitOfWork.Product.Get(p => p.Id == product.Id);
            if (existingProduct == null)
            {
                throw new ProductNotFoundException(product.Id);
            }

            // Update product properties
            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            existingProduct.ProductCode = product.ProductCode;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.MinimumQuantity = product.MinimumQuantity;
            existingProduct.DiscountRate = product.DiscountRate;

            // Handle image updates (if new image provided via domain)
            // Note: Better approach would be to pass IFormFile through DTO
            if (product.ImagePath != null)
            {
                existingProduct.ImagePath = product.ImagePath;
            }

            _unitOfWork.Product.Update(existingProduct);
            await _unitOfWork.Save();
            _logger.LogInformation("Product updated: {ProductId}", product.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", product.Id);
            throw;
        }
    }

    /// <summary>
    /// Deletes a product by ID.
    /// Now properly async (was async void - ANTI-PATTERN).
    /// </summary>
    public async Task DeleteProductAsync(int id)
    {
        try
        {
            var product = await _unitOfWork.Product.Get(p => p.Id == id);
            if (product == null)
            {
                throw new ProductNotFoundException(id);
            }

            // Delete associated image
            if (!string.IsNullOrEmpty(product.ImagePath))
            {
                await DeleteImageAsync(product.ImagePath);
            }

            _unitOfWork.Product.Remove(product);
            await _unitOfWork.Save();
            _logger.LogInformation("Product deleted: {ProductId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            throw;
        }
    }

    public async Task<IReadOnlyList<Product>> GetProductsAsync()
    {
        try
        {
            var products = await _unitOfWork.Product.GetAll();
            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all products");
            throw;
        }
    }

    private async Task DeleteImageAsync(string imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))
            return;

        try
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), imagePath.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("Image deleted: {ImagePath}", fullPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image: {ImagePath}", imagePath);
            // Don't rethrow - image deletion shouldn't fail product deletion
        }
    }

    /// <summary>
    /// Saves a product image asynchronously to the wwwroot/images directory.
    /// </summary>
    private async Task<string?> SaveProductImageAsync(IFormFile image)
    {
        if (image == null || image.Length == 0)
            return null;

        if (image.Length > MaxFileSize)
        {
            throw new InvalidOperationException("File size exceeds 5MB limit.");
        }

        var fileExtension = Path.GetExtension(image.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(fileExtension))
        {
            throw new InvalidOperationException("Only .jpg, .png, and .gif files are allowed.");
        }

        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
        if (!Directory.Exists(uploadsPath))
        {
            Directory.CreateDirectory(uploadsPath);
        }

        var fileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(uploadsPath, fileName);

        try
        {
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }
            _logger.LogInformation("Image saved: {FileName}", fileName);
            return $"/images/{fileName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving image: {FileName}", fileName);
            throw;
        }
    }
}
