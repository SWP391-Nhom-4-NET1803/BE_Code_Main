using ClinicPlatformDTOs.BookingModels;
using ClinicPlatformObjects.BookingModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformRepositories.Contracts
{
    public interface IBookingRepository: IDisposable
    {
        AppointmentInfoModel CreateNewBooking(AppointmentInfoModel booking);
        AppointmentInfoModel? GetBooking(Guid id);
        IEnumerable<AppointmentInfoModel> GetAll();
        AppointmentInfoModel? UpdateBookingInfo(AppointmentInfoModel booking);
        bool DeleteBookingInfo(Guid bookId);

        public BookedServiceInfoModel? GetBookingService(Guid appointmentId);
        public BookedServiceInfoModel? AddBookingService(BookedServiceInfoModel bookedService);
        public BookedServiceInfoModel? UpdateBookingService(BookedServiceInfoModel service);
        public void DeleteBookingService(Guid appointmentId);
    }
}
