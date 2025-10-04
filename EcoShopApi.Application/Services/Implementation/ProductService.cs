using EcoShopApi.Application.Common.Interfaces;
using EcoShopApi.Application.Services.Interface;
using EcoShopApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
namespace EcoShopApi.Application.Services.Implementation
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<Product> GetProductByIdAsync(int id)
        {
            return await Task.FromResult(_unitOfWork.Product.Get(p => p.Id == id));
        }

        public void CreateProductAsync(Product productToCreate)
        {
            _unitOfWork.Product.Add(productToCreate);
            _unitOfWork.Save();
        }
        public void UpdateProductAsync(Product product)
        {
            var existingProduct = _unitOfWork.Product.Get(p => p.Id == product.Id);
            if (existingProduct == null)
            {
                throw new Exception("Product not found");
            }
            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            if (product.ImagePath != null)
            {
                existingProduct.ImagePath = product.ImagePath;
            }
            var oldImagePath = existingProduct.ImagePath;
            if (oldImagePath != null && product.Image != null)
            {
                // Delete old image
                DeleteImage(oldImagePath);
                // Save new image
                var newImagePath = SaveProductImageAsync(product.Image).Result;
            }
            existingProduct.Category = product.Category;
            existingProduct.ProductCode = product.ProductCode;
            existingProduct.MinimumQuantity = product.MinimumQuantity;
            existingProduct.DiscountRate = product.DiscountRate;
            existingProduct.ImagePath = product.ImagePath;
            existingProduct.CategoryId = product.CategoryId;

            _unitOfWork.Product.Update(existingProduct);
            _unitOfWork.Save();
        }
        public void DeleteProductAsync(int id)
        {
            var product = _unitOfWork.Product.Get(p => p.Id == id);
            if (product == null)
            {
                throw new Exception("Product not found");
            }
            _unitOfWork.Product.Remove(product);
            _unitOfWork.Save();


        }
        public async Task<IReadOnlyList<Product>> GetProductsAsync()
        {
            return await Task.FromResult(_unitOfWork.Product.GetAll().ToList());
        }
       
        private void DeleteImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return;

            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), imagePath.TrimStart('/'));

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        public async Task<string?> SaveProductImageAsync(IFormFile image)
        {
            if (image == null || image.Length == 0)
                return null;

            // Validate file size (5MB limit)
            const long maxFileSize = 5 * 1024 * 1024; // 5MB
            if (image.Length > maxFileSize)
            {
                throw new InvalidOperationException("File size exceeds 5MB limit.");
            }

            // Validate file format
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new InvalidOperationException("Only .jpg, .png, and .gif files are allowed.");
            }

            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            // Generate unique filename
            var fileName = Guid.NewGuid() + fileExtension;
            var filePath = Path.Combine(uploadsPath, fileName);

            // Save file asynchronously
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            // Return relative URL path
            return $"/images/{fileName}";
        }
    }
}
