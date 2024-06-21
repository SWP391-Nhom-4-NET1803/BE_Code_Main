namespace ClinicPlatformWebAPI.Controllers
{
    public class SomeRandomIdea
    {
        // Based on the BookingRegistrationModel
        public Guid TimeSlotId { get; set; }
        public DateOnly AppointmentDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public int CustomerId { get; set; }
        public int DentistId { get; set; }
        public int ClinicId { get; set; }
        public string AppointmentType { get; set; } = GeneralBooking;
        public Guid? ServiceId { get; set; } = null;
        public int MaxRecurring { get; set; } = 0; // Not being used. This is the "Total amount" of time reocurring.
        public bool IsRecurring { get; set; } = false; // Not being used.

        // ====================================== Idea 1 ==============================================
        // >> Get all appointment Info (that still not yet come). 
        // >> Run background task to check every 10 minutes.
        // >> If time remainding is smaller than a day.
        // >> Create new appointment on the go based on the previous setup if MaxReccurring not met.
        // ============================================================================================
        public int RecurringDaySpan { get; set; } = 0; // Not being used.
        public int RecurringWeekSpan { get; set; } = 0; // Not being used.
        public int RecurringMonthSpan { get; set; } = 0; // Not being used.
        public int RecurringYearSpan { get; set; } = 0; // Not being used.
        public int RecurringOnWeekday { get; set; } = 0; // Not being used.

        // ====================================== Idea 2 ==============================================
        // What in my head:
        // >> Calculate the amount of can be created.
        // >> Pre-generated the appointment schedule.
        // ============================================================================================
        public int ReccuringSpan { get; set; } = 0; // Not being used.
        public DateOnly StartDate { get; set; } // Not being used.
        public DateOnly EndDate { get; set; } // Not being used.

        // ============================================================================================

        public const string GeneralBooking = "Khám tổng quát";
        public const string ServiceBooking = "Khám dịch vụ";
    }
}
