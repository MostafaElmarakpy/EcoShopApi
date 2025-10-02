using EcoShopApi.Application.Common.Interfaces;
using EcoShopApi.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoShopApi.Infrastructure.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext;

        private readonly IUnitOfWork _unitOfWork;
        public IUserRepository User { get; private set; }
        public IProductRepository Product { get; private set; }
        public UnitOfWork(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;

            User = new UserRepository(_dbContext);
            Product = new ProductRepository(_dbContext);

        }



        public void Save()
        {
            _dbContext.SaveChanges();
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}
