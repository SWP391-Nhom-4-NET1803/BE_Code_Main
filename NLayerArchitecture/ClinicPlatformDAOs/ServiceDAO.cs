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
    public class ServiceDAO: IDisposable
    {
        private readonly DentalClinicPlatformContext _context;
        private bool disposedValue;

        public ServiceDAO()
        {
            _context = new DentalClinicPlatformContext();
        }

        public ServiceDAO(DentalClinicPlatformContext context)
        {
            _context = context;
        }

        public Service AddService(Service Service)
        {
            _context.Add(Service);
            this.SaveChanges();

            return Service;
        }

        public Service? GetService(int serviceId)
        {
            return _context.Services.Where(x => x.ServiceId == serviceId).FirstOrDefault();
        }

        public IEnumerable<Service> GetAllService()
        {
            return _context.Services.ToList();
        }

        public Service UpdateService(Service Service)
        {
            Service? ServiceInfo = GetService(Service.ServiceId);

            if (ServiceInfo != null)
            {
                _context.Services.Update(Service);
                SaveChanges();
            }

            return Service;
        }

        public void DeleteService(int serviceId)
        {
            Service? Service = GetService(serviceId);

            if (Service != null)
            {
                _context.Services.Remove(Service);
                this.SaveChanges();
            }
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

        public IEnumerable<Service> Filter(Expression<Func<Service, bool>> filter, Func<IQueryable<Service>, IOrderedQueryable<Service>>? orderBy, string includeProperties = "", int? pageSize = null, int? pageIndex = null)
        {
            IQueryable<Service> query = _context.Services;

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

