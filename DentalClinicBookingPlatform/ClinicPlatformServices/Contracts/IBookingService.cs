using ClinicPlatformDTOs.BookingModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformServices.Contracts
{
    public interface IBookingService: IDisposable
    {
        bool CreateNewBooking(BookingRegistrationModel bookInfo, out string message);
        bool CreateNewPeriodicBooking(BookingRegistrationModel bookInfo, out string message);
        bool CreateOneTimeBooking(BookingRegistrationModel bookInfo, out string message);

        IEnumerable<BookingModel> GetAllBooking();
        BookingModel? GetBooking(Guid id);
        IEnumerable<BookingModel> GetAllClinicBooking(int clinicId, bool includeCancelled = false);
        IEnumerable<BookingModel> GetAllCustomerBooking(int customerId, bool includeCancelled = false);
        IEnumerable<BookingModel> GetAllDentistBooking(int dentistId, bool includeCancelled = false);
        IEnumerable<BookingModel> GetAllBookingOnDay(DateOnly date);

        bool UpdateBookingInformation(BookingModel bookModel, out string message);
        bool CancelBooking(Guid id, out string message);
        bool ChangeDate(Guid bookId, DateOnly newDate, out string message);
        bool ChangeSlot(Guid bookId, Guid clinicSlotId, out string message);
        bool ChangeDentist(Guid bookId, int clinicDentist, out string message);
        bool ChangeService(Guid bookId, Guid clinicServiceId, out string message);

        bool DeleteBookingInformation(Guid bookId, out string messgae);
        
        IEnumerable<BookingModel> FilterBookList(IEnumerable<BookingModel> list, DateOnly? start = null, DateOnly? end = null, bool includeCanceledOrFinished = false, int? page_size = null, int? page = null);

    }
}
