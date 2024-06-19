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
    public class BookingDAO : IFilterQuery<Booking>, IDisposable
    {
        private readonly DentalClinicPlatformContext _context;
        private bool disposedValue;

        public BookingDAO()
        {
            _context = new DentalClinicPlatformContext();
        }

        public BookingDAO(DentalClinicPlatformContext context)
        {
            _context = context;
        }

        public Booking AddBooking(Booking booking)
        {
            _context.Add(booking);
            this.SaveChanges();

            return booking;
        }

        public Booking? GetBooking(Guid BookId)
        {
            return _context.Bookings.Where(x => x.BookId == BookId).FirstOrDefault();
        }

        public IEnumerable<Booking> GetAllBookings(Guid BookId)
        {
            return _context.Bookings.ToList();
        }

        public Booking UpdateBooking(Booking booking)
        {
            Booking? bookingInfo = GetBooking(booking.BookId);

            if (bookingInfo != null)
            {
                _context.Bookings.Update(booking);
                SaveChanges();
            }

            return booking;
        }

        public void DeleteBooking(Guid bookId)
        {
            Booking? booking = GetBooking(bookId);

            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                this.SaveChanges();
            }
        }

        public void SaveChanges()
        {
            this._context.SaveChanges();
        }

        /// <summary>
        ///     Please consider using this when you are too tired to actually implement good structuring.
        /// </summary>
        /// <param name="BookId"></param>
        /// <returns></returns>
        public Booking? GetFullBookingDetail(Guid BookId)
        {
            return _context.Bookings.Where(x => x.BookId == BookId)
                .Include(x => x.Clinic)
                .Include(x => x.Customer)
                .Include(x => x.Dentist)
                .Include(x => x.ScheduleSlot)
                    .ThenInclude(y => y.Slot)
                .Include(x => x.BookingService)
                .FirstOrDefault();
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

        IEnumerable<Booking> IFilterQuery<Booking>.Filter(Expression<Func<Booking, bool>> filter, Func<IQueryable<Booking>, IOrderedQueryable<Booking>>? orderBy, string includeProperties, int? pageSize, int? pageIndex)
        {
            IQueryable<Booking> query = _context.Bookings;

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
