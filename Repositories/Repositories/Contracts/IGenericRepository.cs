using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Repositories.Contracts
{
    public interface IGenericRepository<T, X> where T : class
    {
        IEnumerable<T> GetAll();

        IEnumerable<T> GetAll(Expression<Func<T, bool>> filter, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy, string includeProperties = "", int? pageSize = null, int? pageIndex = null);

        T GetById(X id);

        void Add(T entity);

        void Update(T entity);

        void Delete(T entity);

        int Count(Expression<Func<T, bool>>? filter);

        void Save();
    }
}
