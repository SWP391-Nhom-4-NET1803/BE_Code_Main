using ClinicPlatformDatabaseObject;
using ClinicPlatformDTOs.BookingModels;
using ClinicPlatformObjects.BookingModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformServices.Contracts
{
    public interface IBookingService: IDisposable
    {
        AppointmentInfoModel? CreateNewBooking(AppointmentRegistrationModel bookInfo, out string message);
        AppointmentInfoModel? CreateNewPeriodicBooking(AppointmentRegistrationModel bookInfo, AppointmentSetting setting,  out string message);

        IEnumerable<AppointmentInfoModel> GetAllBooking();
        AppointmentInfoModel? GetBooking(Guid id);
        IEnumerable<AppointmentInfoModel> GetAllClinicBooking(int clinicId, bool includeCancelled = false);
        IEnumerable<AppointmentInfoModel> GetAllCustomerBooking(int customerId, bool includeCancelled = false);
        IEnumerable<AppointmentInfoModel> GetAllDentistBooking(int dentistId, bool includeCancelled = false);
        IEnumerable<AppointmentInfoModel> GetAllBookingOnDay(DateOnly date);
        AppointmentInfoModel? CancelBooking(Guid id, out string message);
        AppointmentInfoModel? ChangeDate(Guid bookId, DateOnly newDate, out string message);
        AppointmentInfoModel? ChangeSlot(Guid bookId, Guid clinicSlotId, out string message);
        AppointmentInfoModel? ChangeAppointmentTime(out string message, Guid bookId, DateOnly? newDate = null, Guid? slotId = null);
        AppointmentInfoModel? ChangeDentist(Guid bookId, int clinicDentist, out string message);
        AppointmentInfoModel? ChangeService(Guid bookId, Guid clinicServiceId, out string message);
        AppointmentInfoModel? RemoveService(Guid bookId, out string message);
        BookedServiceInfoModel? GetBookedService(Guid bookId);

        bool DeleteBookingInformation(Guid bookId, out string messgae);
        
        IEnumerable<AppointmentInfoModel> FilterBookList(IEnumerable<AppointmentInfoModel> list, DateOnly? start = null, DateOnly? end = null, bool includeCanceledOrFinished = false, int? page_size = null, int? page = null);

    }
}
