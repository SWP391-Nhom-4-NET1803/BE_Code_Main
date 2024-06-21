using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformDTOs.BookingModels
{
    public class BookingRegistrationModel
    {
        public Guid TimeSlotId { get; set; }
        public DateOnly AppointmentDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public int CustomerId { get; set; }
        public int DentistId { get; set; }
        public int ClinicId { get; set; }
        public string AppointmentType { get; set; } = GeneralBooking;
        public Guid? ServiceId { get; set; } = null;
        public int MaxRecurring { get; set; } = 0; // Not being used. This is the "Total amount" of time reocurring.
        public bool IsRecurring { get; set; } = false; // Not being used.

        public const string GeneralBooking = "Khám t?ng quát";
        public const string ServiceBooking = "Khám d?ch v?";

    }
}
