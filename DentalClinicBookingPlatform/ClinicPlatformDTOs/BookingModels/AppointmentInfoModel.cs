using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformDTOs.BookingModels
{
    public class AppointmentInfoModel
    {
        public Guid Id { get; set; }
        public int PatientNumber { get; set; }
        public string Type { get; set; } = null!;
        public Guid ClinicSlotId { get; set; }
        public DateOnly AppointmentDate { get; set; }
        public DateTime CreationTime { get; set; }
        public bool Status { get; set; } = false;
        public int CustomerId { get; set; }
        public int DentistId { get; set; }
        public int ClinicId { get; set; }
        public Guid SelectedServiceId { get; set; }
        public Guid OriginalAppoinment {  get; set; }
        public Guid? PaymentId { get; set; }
        public int AppointmentFee { get; set; }
    }
}
