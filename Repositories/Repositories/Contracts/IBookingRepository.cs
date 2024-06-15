using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Repositories.Contracts
{
    public interface IBookingRepository
    {
        IEnumerable<Booking> GetBookingForUser(int userId);
        IEnumerable<Booking> getBookingForCustomer(int customerId);
        IEnumerable<Booking> getBookingForClinicStaff(int staffId);
        IEnumerable<Booking> getClinicBooking(int clinicId, bool futureOnly);
        IEnumerable<Booking> getClinicBookingInDateRange(int clinicId, DateOnly startDate, DateOnly endDate);
        IEnumerable<Booking> getBookingInDateRange(int userId, DateOnly startDate, DateOnly endDate);
        IEnumerable<Booking> getFutureBooking(int userId);
        public Booking? getFullBookingInfo(Guid bookId);
    }
}
