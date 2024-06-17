using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformDTOs.BookingModels
{
    public class BookingRegistrationModel
    {
        public Guid? TimeSlotId { get; set; } = null;
        public DateOnly AppointmentDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public int CustomerId { get; set; }
        public int DentistId { get; set; }
        public int ClinicId { get; set; }
        public int? ServiceId { get; set; } = null;
        public int RepeatCount { get; set; } = 0;
        public bool IsRecurring { get; set; } = false;
    }
}
