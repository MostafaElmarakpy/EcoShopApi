using EcoShopApi.Application.Common.Interfaces;
using EcoShopApi.Domain.Entities;
using EcoShopApi.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoShopApi.Infrastructure.Repository
{
    public class ProductRepository : GenaricRepository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Product entity)
        {
            _db.Products.Update(entity);
        }

    }
}
