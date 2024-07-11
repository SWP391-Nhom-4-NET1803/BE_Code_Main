using ClinicPlatformDatabaseObject;
using ClinicPlatformDTOs.BookingModels;
using ClinicPlatformDTOs.UserModels;
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
        /// <summary>
        /// Create a new booking information.
        /// </summary>
        /// <param name="bookInfo">Appointment information</param>
        /// <param name="message">the result output message</param>
        /// <returns><see cref="AppointmentInfoModel"/> with the provided appointment information.</returns>
        AppointmentInfoModel? CreateNewBooking(AppointmentRegistrationModel bookInfo, out string message);

        /// <summary>
        ///  Get all booking information
        ///  Note: Don't use this.
        /// </summary>
        /// <returns>All booking information in the database</returns>
        IEnumerable<AppointmentInfoModel> GetAllBooking();

        /// <summary>
        ///  Get a specific booking information.
        /// </summary>
        /// <param name="id"></param>
        /// <returns><see cref="AppointmentInfoModel"/> of the appointment if found in the database with the given Id, else return null</returns>
        AppointmentInfoModel? GetBooking(Guid id);

        /// <summary>
        /// Return all appointment that occurs in a clinic.
        /// </summary>
        /// <param name="clinicId">The clinic Id</param>
        /// <param name="includeCancelled">To include all canceled appointment</param>
        /// <returns>A list of all appointment belong to a clinic with the provided clinic Id.</returns>
        IEnumerable<AppointmentInfoModel> GetAllClinicBooking(int clinicId, bool includeCancelled = false);

        /// <summary>
        /// Return all appointment for a customer.
        /// </summary>
        /// <param name="customerId">The customer Id </param>
        /// <param name="includeCancelled">To include all canceled appointment</param>
        /// <returns>A list of all appointment for a customer with provided customer Id</returns>
        IEnumerable<AppointmentInfoModel> GetAllCustomerBooking(int customerId, bool includeCancelled = false);

        /// <summary>
        ///  Return all appointment for a clinic dentist.
        /// </summary>
        /// <param name="dentistId">Dentist Id</param>
        /// <param name="includeCancelled">To include all canceled appointment</param>
        /// <returns>A list of all appointment for dentist with provided dentist Id</returns>
        IEnumerable<AppointmentInfoModel> GetAllDentistBooking(int dentistId, bool includeCancelled = false);

        /// <summary>
        /// Get all booking information on a specific date
        /// </summary>
        /// <param name="date">The input date to get information</param>
        /// <param name="includeCanclled">To include all canceled appointment</param>
        /// <returns>A list of all appointment in a single date for the given input</returns>
        IEnumerable<AppointmentInfoModel> GetAllBookingOnDay(DateOnly date, bool includeCanclled = false);

        /// <summary>
        ///  Cancel an existing booked appointment.
        ///  Will always return null if tries to update a finished or canceled appointment.
        /// </summary>
        /// <param name="id">The appointment Id</param>
        /// <param name="message">The operation output message</param>
        /// <returns><see cref="AppointmentInfoModel"/> if successfully canceled the appointment, else <see cref="null"/></returns>
        AppointmentInfoModel? CancelBooking(Guid id, out string message);


        /// <summary>
        ///  Mark an existing booked appointment to finished.
        ///  Will always return null if tries to update a finished or canceled appointment.
        /// </summary>
        /// <param name="id">The appointment Id</param>
        /// <param name="message">The operation output message</param>
        /// <returns><see cref="AppointmentInfoModel"/> if successfully canceled the appointment, else <see cref="null"/></returns>
        AppointmentInfoModel? FinishBooking(Guid id, out string message);

        /// <summary>
        ///  Change the date of an existing booked appointment.
        ///  Will always return null if tries to update a finished or canceled appointment.
        /// </summary>
        /// <param name="bookId">The appointment Id</param>
        /// <param name="newDate">New date to change to</param>
        /// <param name="message">The operation output message</param>
        /// <returns><see cref="AppointmentInfoModel"/> if successfully canceled the appointment, else <see cref="null"/></returns>
        AppointmentInfoModel? ChangeDate(Guid bookId, DateOnly newDate, out string message);

        /// <summary>
        /// Change the selected time slot for an existing booked appointment.
        /// Will always return null if tries to update a finished or canceled appointment.
        /// </summary>
        /// <param name="bookId">The appointment Id</param>
        /// <param name="newDate">New date to change to</param>
        /// <param name="message">The operation output message</param>
        /// <returns><see cref="AppointmentInfoModel"/> if successfully canceled the appointment, else <see cref="null"/></returns>
        AppointmentInfoModel? ChangeSlot(Guid bookId, Guid clinicSlotId, out string message);
        AppointmentInfoModel? ChangeAppointmentTime(out string message, Guid bookId, DateOnly? newDate = null, Guid? slotId = null);
        AppointmentInfoModel? ChangeDentist(Guid bookId, int clinicDentist, out string message);
        AppointmentInfoModel? ChangeService(Guid bookId, Guid clinicServiceId, out string message);
        AppointmentInfoModel? RemoveService(Guid bookId, out string message);
        AppointmentInfoModel? SetAppoinmentStatus(Guid bookId, string status, out string message);
        AppointmentInfoModel? SetAppointmentNote(Guid bookId, string note, out string message);

        BookedServiceInfoModel? GetBookedService(Guid bookId);

        bool DeleteBookingInformation(Guid bookId, out string messgae);

        public bool DentistIsAvailableOn(DateOnly date, Guid slotId, int dentistId, out string message);

        public bool DentistIsFreeForCheckupOn(DateOnly date, Guid slotId, int dentistId, out string message);
        public bool DentistIsFreeForTreatmentOn(DateOnly date, Guid slotId, int dentistId, out string message);
        IEnumerable<UserInfoModel> GetDentistFreeForCheckup(int clinicId, DateOnly date, Guid slotId);

        IEnumerable<AppointmentInfoModel> FilterBookList(IEnumerable<AppointmentInfoModel> list, DateOnly? start = null, DateOnly? end = null, bool includeCanceledOrFinished = false, int? page_size = null, int? page = null);

    }
}
