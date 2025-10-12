using EcoShopApi.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoShopApi.Application.Services.Interface
{
    public interface IProductService
    {
        Task<IReadOnlyList<Product>> GetProductsAsync();
        Task<Product> GetProductByIdAsync(int id);

        void CreateProductAsync(Product productToCreate ,IFormFile? files);
        void UpdateProductAsync(Product product);
        void DeleteProductAsync(int id);


    }
}
