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
    public class ClinicDAO: IFilterQuery<Clinic>, IDisposable
    {
        private readonly DentalClinicPlatformContext _context;
        private bool disposedValue;

        public ClinicDAO()
        {
            _context = new DentalClinicPlatformContext();
        }

        public ClinicDAO(DentalClinicPlatformContext context)
        {
            _context = context;
        }

        public Clinic AddClinic(Clinic clinic)
        {
            this._context.Add(clinic);
            SaveChanges();
            return clinic;
        }

        public Clinic? GetClinic(int id) 
        {
            return _context.Clinics
                .Where(x => x.ClinicId == id)
                .FirstOrDefault();
        }

        public IEnumerable<Clinic> GetAllClinic()
        {
            return _context.Clinics.ToList();
        }

        public Clinic UpdateClinic(Clinic clinic)
        {
            Clinic? clinicInfo = GetClinic(clinic.ClinicId);

            if (clinicInfo != null)
            {
                _context.Clinics.Update(clinic);
                SaveChanges();
            }

            return clinic;
        }

        public void DeleteClinic(int userId)
        {
            Clinic? clinic = GetClinic(userId);

            if (clinic != null)
            {
                _context.Clinics.Remove(clinic);
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
                    this._context.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public IEnumerable<Clinic> Filter(Expression<Func<Clinic, bool>> filter, Func<IQueryable<Clinic>, IOrderedQueryable<Clinic>>? orderBy, string includeProperties = "", int? pageSize = null, int? pageIndex = null)
        {
            IQueryable<Clinic> query = _context.Clinics;

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
