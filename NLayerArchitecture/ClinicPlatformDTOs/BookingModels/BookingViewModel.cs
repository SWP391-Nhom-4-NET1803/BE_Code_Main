using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformDTOs.BookingModels
{
    public class BookingViewModel
    {
        public Guid BookId { get; set; }
        public string? appointmentType { get; set; }
        public string? CustomerFullName { get; set; }
        public string? DentistFullname { get; set; }
        public DateOnly AppointmentDate { get; set; }
        public DateTime CreationTime { get; set; }
        public TimeOnly AppointmentTime { get; set; }
        public TimeOnly? ExpectedEndTime { get; set; }
        public string? ClinicName { get; set; }
        public string? ClinicAddress { get; set; }
        public string? ClinicPhone { get; set; }
        public string? SelectedServiceName { get; set; }
    }
}
