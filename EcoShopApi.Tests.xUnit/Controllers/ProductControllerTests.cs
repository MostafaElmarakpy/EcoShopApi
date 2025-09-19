using EcoShopApi.Application.Services.Interface;
using EcoShopApi.Controllers;
using EcoShopApi.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EcoShopApi.Tests.xUnit.Controllers
{
    public class ProductControllerTests
    {
        private Mock<IProductService> _productServiceMock;
        private Mock<ILogger<ProductsController>> _loggerMock;
        private ProductsController _productController;

        public ProductControllerTests()
        {
            _productServiceMock = new Mock<IProductService>();
            _loggerMock = new Mock<ILogger<ProductsController>>();
            _productController = new ProductsController(_productServiceMock.Object);
        }

        [Fact]
        public async Task GetProducts_ReturnsOkResult_WithListOfProducts()
        {
            // Arrange:
            var products = new List<Product> { new Product { Id = 1, Name = "TestProduct", Price = 100, ProductCode = " P01" } };

            _productServiceMock.Setup(s => s.GetProductsAsync()).ReturnsAsync(products);

            // Act: 
            var result = await _productController.GetAll();
            //var result = await _controller.GetProducts();

            // Assert: 
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnProducts = Assert.IsType<List<Product>>(okResult.Value);
            Assert.Single(returnProducts);
            Assert.Equal(products, returnProducts);

        }

        [Fact]
        public async Task GetProductById_ReturnsOkResult_WithProduct()
        {
            // Arrange:
            var productId = 1;
            var product = new Product { Id = productId, Name = "TestProduct", Price = 100, ProductCode = " P01" };

            _productServiceMock.Setup(s => s.GetProductByIdAsync(productId)).ReturnsAsync(product);

            // Act: 
            var result = await _productController.GetById(productId);

            // Assert: 
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnProduct = Assert.IsType<Product>(okResult.Value);
            Assert.Equal(product, returnProduct);
        }
        [Fact]
        public async Task GetProductById_ReturnsNotFound_WhenProductDoesNotExist()
        {
            // Arrange:
            var productId = 1;

            _productServiceMock.Setup(s => s.GetProductByIdAsync(productId)).ReturnsAsync((Product)null);

            // Act: 
            var result = await _productController.GetById(productId);

            // Assert: 
            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public void CreateProduct_ReturnsCreatedAtActionResult_WithCreatedProduct()
        {
            // Arrange:
            var product = new Product { Id = 1, Name = "TestProduct", Price = 100, ProductCode = " P01" };

            // Act: 
            var result = _productController.Create(product);

            // Assert: 
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnProduct = Assert.IsType<Product>(createdAtActionResult.Value);
            Assert.Equal(product, returnProduct);
            Assert.Equal(nameof(_productController.GetById), createdAtActionResult.ActionName);
            Assert.Equal(product.Id, createdAtActionResult.RouteValues["id"]);
        }
        [Fact]
        public void UpdateProduct_ReturnsNoContent_WhenProductIsUpdated()
        {
            // Arrange:
            var productId = 1;
            var product = new Product { Id = productId, Name = "UpdatedProduct", Price = 150, ProductCode = " P01" };

            _productServiceMock.Setup(s => s.UpdateProductAsync(product));

            // Act: 
            var result = _productController.Update(productId, product);

            // Assert: 
            Assert.IsType<NoContentResult>(result);
            _productServiceMock.Verify(s => s.UpdateProductAsync(product), Times.Once);
        }
        [Fact]
        public void UpdateProduct_ReturnsBadRequest_WhenIdMismatch()
        {
            // Arrange:
            var productId = 1;
            var product = new Product { Id = 2, Name = "UpdatedProduct", Price = 150, ProductCode = " P01" };

            // Act: 
            var result = _productController.Update(productId, product);

            // Assert: 
            Assert.IsType<BadRequestResult>(result);
        }
        [Fact]
        public void UpdateProduct_ReturnsNotFound_WhenProductDoesNotExist()
        {
            // Arrange:
            var productId = 1;
            var product = new Product { Id = productId, Name = "UpdatedProduct", Price = 150, ProductCode = " P01" };

            _productServiceMock.Setup(s => s.UpdateProductAsync(product)).Throws(new Exception("Product not found"));

            // Act: 
            var result = _productController.Update(productId, product);

            // Assert: 
            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public void DeleteProduct_ReturnsNoContent_WhenProductIsDeleted()
        {
            // Arrange:
            var productId = 1;
            var product = new Product { Id = productId, Name = "TestProduct", Price = 100, ProductCode = " P01" };

            _productServiceMock.Setup(s => s.GetProductByIdAsync(productId)).ReturnsAsync(product);
            _productServiceMock.Setup(s => s.DeleteProductAsync(productId));

            // Act: 
            var result = _productController.Delete(productId);

            // Assert: 
            Assert.IsType<NoContentResult>(result);
            _productServiceMock.Verify(s => s.DeleteProductAsync(productId), Times.Once);
        }
        [Fact]
        public void DeleteProduct_ReturnsNotFound_WhenProductDoesNotExist()
        {
            // Arrange:
            var productId = 1;

            _productServiceMock.Setup(s => s.GetProductByIdAsync(productId)).ReturnsAsync((Product)null);

            // Act: 
            var result = _productController.Delete(productId);

            // Assert: 
            Assert.IsType<NotFoundResult>(result);
        }






    }
}
