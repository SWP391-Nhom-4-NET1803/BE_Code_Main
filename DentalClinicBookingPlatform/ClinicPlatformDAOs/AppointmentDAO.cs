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
    public class AppointmentDAO : IFilterQuery<Appointment>, IDisposable
    {
        private readonly DentalClinicPlatformContext _context;
        private bool disposedValue;

        public AppointmentDAO()
        {
            _context = new DentalClinicPlatformContext();
        }

        public AppointmentDAO(DentalClinicPlatformContext context)
        {
            _context = context;
        }

        public bool AddAppointments(Appointment Appointments)
        {
            _context.Add(Appointments);
            this.SaveChanges();

            return true;
        }

        public Appointment? GetAppointments(Guid BookId)
        {
            return _context.Appointments.Where(x => x.Id == BookId)
                .Include(x => x.BookedService)
                .FirstOrDefault();
        }

        public IEnumerable<Appointment> GetAll()
        {
            return _context.Appointments.Include(BookedService).ToList();
        }

        public bool UpdateAppointments(Appointment Appointments)
        {
            Appointment? AppointmentsInfo = GetAppointments(Appointments.Id);

            if (AppointmentsInfo != null)
            {
                _context.Appointments.Update(Appointments);
                SaveChanges();

                return true;
            }

            return false;
        }

        public bool DeleteAppointments(Guid bookId)
        {
            Appointment? Appointments = GetAppointments(bookId);

            if (Appointments != null)
            {
                _context.Appointments.Remove(Appointments);
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

        IEnumerable<Appointment> IFilterQuery<Appointment>.Filter(Expression<Func<Appointment, bool>> filter, Func<IQueryable<Appointment>, IOrderedQueryable<Appointment>>? orderBy, string includeProperties, int? pageSize, int? pageIndex)
        {
            IQueryable<Appointment> query = _context.Appointments;

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
