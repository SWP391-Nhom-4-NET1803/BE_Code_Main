using ClinicPlatformBusinessObject;
using ClinicPlatformDAOs.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformDAOs
{
    public class ClinicServiceDAO: IFilterQuery<ClinicService>, IDisposable
    {
        private readonly DentalClinicPlatformContext _context;
        private bool disposedValue;

        public ClinicServiceDAO()
        {
            _context = new DentalClinicPlatformContext();
        }

        public ClinicServiceDAO(DentalClinicPlatformContext context)
        {
            _context = context;
        }

        public ClinicService AddClinicService(ClinicService ClinicService)
        {
            _context.Add(ClinicService);
            this.SaveChanges();

            return ClinicService;
        }

        public ClinicService? GetClinicService(Guid serviceId)
        {
            return _context.ClinicServices.Where(x => x.ClinicServiceId == serviceId).FirstOrDefault();
        }

        public IEnumerable<ClinicService> GetAllClinicService()
        {
            return _context.ClinicServices.ToList();
        }

        public ClinicService UpdateClinicService(ClinicService clinicService)
        {
            ClinicService? ClinicServiceInfo = GetClinicService(clinicService.ClinicServiceId);

            if (ClinicServiceInfo != null)
            {
                _context.ClinicServices.Update(clinicService);
                SaveChanges();
            }

            return clinicService;
        }

        public void DeleteClinicService(Guid serviceId)
        {
            ClinicService? clinicService = GetClinicService(serviceId);

            if (clinicService != null)
            {
                _context.ClinicServices.Remove(clinicService);
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

        public IEnumerable<ClinicService> Filter(Expression<Func<ClinicService, bool>> filter, Func<IQueryable<ClinicService>, IOrderedQueryable<ClinicService>>? orderBy, string includeProperties = "", int? pageSize = null, int? pageIndex = null)
        {
            IQueryable<ClinicService> query = _context.ClinicServices;

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
