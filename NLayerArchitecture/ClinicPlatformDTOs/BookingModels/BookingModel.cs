using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformDTOs.BookingModels
{
    public class BookingModel
    {
        [Required(ErrorMessage = "Booking Item must have an ID")]
        public Guid Id { get; set; }
        public string? Type { get; set; }
        public Guid? TimeSlotId { get; set; }
        public DateOnly? AppointmentDate { get; set; }
        public DateTime? CreationTime { get; set; }
        public bool Status { get; set; } = false;
        public int? CustomerId { get; set; }
        public int? DentistId { get; set; }
        public int? ClinicId { get; set; }
        public Guid? SelectedService { get; set; }
    }
}
