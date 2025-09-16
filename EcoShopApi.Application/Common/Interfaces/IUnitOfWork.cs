using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoShopApi.Application.Common.Interfaces
{
    public interface IUnitOfWork
    {
       
        IUserRepository User { get; }
        IProductRepository Product { get; }
        void Save();
        
    }
}
