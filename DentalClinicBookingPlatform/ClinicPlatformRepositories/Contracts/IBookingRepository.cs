using ClinicPlatformBusinessObject;
using ClinicPlatformDTOs.BookingModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformRepositories.Contracts
{
    public interface IBookingRepository: IDisposable
    {
        bool CreateNewBooking(BookingModel booking);
        BookingModel? GetBooking(Guid id);
        IEnumerable<BookingModel> GetAll();
        bool UpdateBookingInfo(BookingModel booking);
        bool DeleteBookingInfo(Guid bookId);
    }
}
