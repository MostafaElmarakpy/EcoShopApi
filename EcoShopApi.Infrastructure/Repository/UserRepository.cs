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
    public class UserRepository : GenaricRepository<AppUser>, IUserRepository
    {
        private readonly ApplicationDbContext _db;
        public UserRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
    }
}
