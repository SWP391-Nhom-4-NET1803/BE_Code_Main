using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.HttpModels.ObjectModels.BookingModels
{
    public class BookingModel
    {
        // Thông tin client trả về để thực hiện yêu cầu.
        public Guid Id { get; set; }
        public string? Type { get; set; }
        public Guid timeSlotId {  get; set; }
        public DateOnly appointmentDate { get; set; }
        public DateTime CreationTime { get; set; }
        public string? Status { get; set; }
        public int? CustomerId { get; set; }
        public int? DentistId { get; set; }
        public int? ClinicId { get; set; }

        public Guid? SelectedService { get; set; }

        // Thông tin trả về client
        public string? CustomerFullName { get; set; }
        public string? DentistFullname { get; set; }
        public TimeOnly AppointmentTime { get; set; }
        public TimeOnly? ExpectedEndTime { get; set; }
        public string? ClinicName { get; set; }
        public string? ClinicAddress { get; set; }
        public string? ClinicPhone { get; set; }
        public string? SelectedServiceName {  get; set; }
        
    }
}
