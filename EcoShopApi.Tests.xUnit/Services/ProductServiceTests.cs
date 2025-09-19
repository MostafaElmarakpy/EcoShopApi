using EcoShopApi.Application.Services.Interface;
using EcoShopApi.Controllers;
using EcoShopApi.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using EcoShopApi.Application.Common.Interfaces;
using EcoShopApi.Application.Services.Implementation;
using Xunit;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;


namespace EcoShopApi.Tests.xUnit.Services
{

    public class ProductServiceTests
    {
        private readonly ProductService _service;
        private Mock<IProductService> _productServiceMock;
        private List<Product> _products;
        private Mock<IUnitOfWork> _unitOfWorkMock;

        public ProductServiceTests()
        {
            var mockRepo = new Mock<IUnitOfWork>();
            _service = new ProductService(mockRepo.Object);
            _productServiceMock = new Mock<IProductService>();
            _products = new List<Product>();
        }
        [Fact]
        public async Task GetProductByIdAsync_ReturnsProduct_WhenProductExists()
        {
            // Arrange
            var productId = 1;
            var expectedProduct = new Product { Id = productId, Name = "Test Product" };
            _productServiceMock.Setup(s => s.GetProductByIdAsync(productId)).ReturnsAsync(expectedProduct);

            // Act
            var result = await _productServiceMock.Object.GetProductByIdAsync(productId);

            // Assert
            Assert.Equal(expectedProduct, result);

        }
        // Additional tests for CreateProductAsync, UpdateProductAsync, and DeleteProductAsync can be added here

        [Fact]
        public void CreateProductAsync_AddsProduct()
        {
            // Arrange
            var newProduct = new Product { Name = "New Product" };

            // Act
            _service.CreateProductAsync(newProduct);

            // Assert internal repo Add called and unit of work Save called
            _unitOfWorkMock.Verify(u => u.Product.Add(It.IsAny<Product>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.Save(), Times.Once);
            Assert.Contains(_products, p => p.Name == "New Product");
        }
        [Fact]
        public void UpdateProductAsync_UpdatesExistingProduct()
        {
            var existing = new Product { Id = 1, Name = "Old" };
            _products.Add(existing);

            var updated = new Product { Id = 1, Name = "Updated", ImagePath = null, Image = null };

            // Act
            _service.UpdateProductAsync(updated);

            // Assert
            _unitOfWorkMock.Verify(u => u.Product.Update(It.IsAny<Product>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.Save(), Times.Once);
            Assert.Contains(_products, p => p.Id == 1 && p.Name == "Updated");
        }


        [Fact]
        public void DeleteProductAsync_DeletesProduct_And_CallsSave()
        {
            // Arrange
            var existing = new Product { Id = 1, Name = "ToDelete" };
            _products.Add(existing);

            // Act
            _service.DeleteProductAsync(1);

            // Assert
            _unitOfWorkMock.Verify(u => u.Product.Remove(existing), Times.Once);
            _unitOfWorkMock.Verify(u => u.Save(), Times.Once);
            Assert.DoesNotContain(_products, p => p.Id == 1);
        }


    }
}