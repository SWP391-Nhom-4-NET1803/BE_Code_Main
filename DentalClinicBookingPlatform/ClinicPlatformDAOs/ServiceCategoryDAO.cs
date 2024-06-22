using ClinicPlatformBusinessObject;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformDAOs
{
    public class ServiceCategoryDAO: IDisposable
    {
        private readonly DentalClinicPlatformContext _context;
        private bool disposedValue;

        public ServiceCategoryDAO()
        {
            _context = new DentalClinicPlatformContext();
        }

        public ServiceCategoryDAO(DentalClinicPlatformContext context)
        {
            _context = context;
        }

        public bool AddCategory(ServiceCategory ServiceCategory)
        {
            _context.Add(ServiceCategory);
            this.SaveChanges();

            return true;
        }

        public ServiceCategory? GetCategory(int categoryId)
        {
            return _context.ServiceCategories.Where(x => x.Id == categoryId).FirstOrDefault();
        }

        public IEnumerable<ServiceCategory> GetAll()
        {
            return _context.ServiceCategories.ToList();
        }

        public ServiceCategory UpdateCategory(ServiceCategory ServiceCategory)
        {
            ServiceCategory? ServiceInfo = GetCategory(ServiceCategory.Id);

            if (ServiceInfo != null)
            {
                _context.ServiceCategories.Update(ServiceCategory);
                SaveChanges();
            }

            return ServiceCategory;
        }

        public bool DeleteCategory(int serviceId)
        {
            ServiceCategory? ServiceCategory = GetCategory(serviceId);

            if (ServiceCategory != null)
            {
                _context.ServiceCategories.Remove(ServiceCategory);
                this.SaveChanges();

                return true;
            }

            return false;
        }

        public void SaveChanges()
        {
            this._context.SaveChanges();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public IEnumerable<ServiceCategory> Filter(Expression<Func<ServiceCategory, bool>> filter, Func<IQueryable<ServiceCategory>, IOrderedQueryable<ServiceCategory>>? orderBy, string includeProperties = "", int? pageSize = null, int? pageIndex = null)
        {
            IQueryable<ServiceCategory> query = _context.ServiceCategories;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            // Implementing pagination
            if (pageIndex.HasValue && pageSize.HasValue)
            {
                // Ensure the pageIndex and pageSize are valid
                int validPageIndex = pageIndex.Value > 0 ? pageIndex.Value - 1 : 0;
                int validPageSize = pageSize.Value > 0 ? pageSize.Value : 10; // Assuming a default pageSize of 10 if an invalid value is passed

                query = query.Skip(validPageIndex * validPageSize).Take(validPageSize);
            }

            return query.ToList();
        }
    }
}

