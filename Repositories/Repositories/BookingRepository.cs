using PlatformRepository.Repositories;
using Repositories.Models;
using Repositories.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Repositories
{
    public class BookingRepository : GenericRepository<Booking, Guid>, IBookingRepository
    {

        public BookingRepository(DentalClinicPlatformContext context) : base(context) { }

        public IEnumerable<Booking> getBookingForUser(string userId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Booking> getBookingInDateRange(DateOnly startDate, DateOnly endDate)
        {
            throw new NotImplementedException();
        }

        public Booking getLatestBooking(string userId)
        {
            throw new NotImplementedException();
        }
    }
}
