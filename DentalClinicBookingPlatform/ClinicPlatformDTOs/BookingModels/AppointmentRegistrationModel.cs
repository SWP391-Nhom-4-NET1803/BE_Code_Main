using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformDTOs.BookingModels
{
    public class AppointmentRegistrationModel
    {
        public Guid TimeSlotId { get; set; }
        public string AppointmentType { get; set; } = Checkup;
        public DateOnly AppointmentDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public int CustomerId { get; set; }
        public int DentistId { get; set; }
        public int ClinicId { get; set; }
        public Guid ServiceId { get; set; }
        public int MaxRecurring { get; set; } = 0;
        public Guid? OrginialAppointment { get; set; } = null!;
        public string Status { get; set; } = "booked";

        public const string Checkup = "checkup";
        public const string Treatment = "treatment";

    }
}
