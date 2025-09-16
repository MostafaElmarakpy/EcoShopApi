using EcoShopApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoShopApi.Application.Services.Interface
{
    public interface IProductService
    {
        Task<Product> GetProductByIdAsync(int id);
        void CreateProductAsync(Product productToCreate);
        void UpdateProductAsync(Product product);
        void DeleteProductAsync(int id);

        Task<IReadOnlyList<Product>> GetProductsAsync();

    }
}
