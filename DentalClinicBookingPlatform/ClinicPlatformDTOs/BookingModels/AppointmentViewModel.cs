using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformDTOs.BookingModels
{
    public class AppointmentViewModel
    {
        public Guid BookId { get; set; }
        public string AppointmentType { get; set; } = null!;
        public string CustomerFullName { get; set; } = null!;
        public string DentistFullname { get; set; } = null!;
        public DateOnly AppointmentDate { get; set; }
        public DateTime CreationTime { get; set; }
        public TimeOnly AppointmentTime { get; set; }
        public TimeOnly ExpectedEndTime { get; set; }
        public string DentistNote { get; set; } = null!;
        public string ClinicName { get; set; } = null!;
        public string ClinicAddress { get; set; } = null!;
        public string ClinicPhone { get; set; } = null!;
        public string SelectedServiceName { get; set; } = null!;
        public int FinalFee { get; set; }
        public bool IsRecurring { get; set; }
        public string BookingStatus { get; set; } = null!;

        // Ids for other reasons
        public int CustomerId { get; set; }
        public int DentistId { get; set; }
        public int ClinicId { get; set; }
        public Guid SlotId { get; set; }
        public Guid ServiceId { get; set; }
        public Guid? OriginalAppointment {  get; set; }
    }
}
