using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformDAOs.Contracts
{
    internal interface IFilterQuery<T>
    {
        IEnumerable<T> Filter(Expression<Func<T, bool>> filter, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy, string includeProperties = "", int? pageSize = null, int? pageIndex = null);
    }
}
