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
        IEnumerable<Booking> getBookingForUser(string userId);

        IEnumerable<Booking> getBookingInDateRange(DateOnly startDate, DateOnly endDate);

        Booking getLatestBooking(string userId);
    }
}
