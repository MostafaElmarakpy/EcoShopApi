using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EcoShopApi.Application.Interfaces
{
    public interface IGenaricRepository<T> where T : class
    {
        Task<List<T>> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperty = null);
        Task<T?> Get(Expression<Func<T, bool>> filter, string? includeProperty = null);
        Task<bool> Any(Expression<Func<T, bool>> filter);
        Task Add(T entity);
        void Remove(T entity);

    }
}
