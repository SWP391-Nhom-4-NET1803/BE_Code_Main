using Core.HttpModels.ObjectModels.BookingModels;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.BookingService
{
    public interface IBookingService: IDisposable
    {
        Boolean CreateSimpleAppointment(BookingModel bookInfo, IEnumerable<ClinicService> services, out string message);

        Boolean CreateOneTimeAppointment(BookingModel bookInfo, out string message);

        Boolean CreateOneTimeAppointment(BookingRegistrationModel bookInfo, out string message);

        IEnumerable<ScheduledSlot> GetFreeSlotOnDay(DateOnly date, int clinicId);

        IEnumerable<ScheduledSlot> GetAllSlotForDay(DateOnly date, int clinicId);

        IEnumerable<ScheduledSlot> GetSlotForWeekDay(int weekday, int clinicId);

        IEnumerable<ScheduledSlot> GetSlotFreeInRange(DateOnly start, DateOnly end, int clinicId);

        IEnumerable<Booking> getClinicBooking(int clinicId, bool onlyFuture);
    }
}
