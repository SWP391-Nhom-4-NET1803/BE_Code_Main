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
        public string Type { get; set; } = null!;
        public Guid ClinicSlotId { get; set; }
        public DateOnly AppointmentDate { get; set; }
        public DateTime CreationTime { get; set; }
        public string Status { get; set; } = null!;
        public int CustomerId { get; set; }
        public int DentistId { get; set; }
        public int ClinicId { get; set; }
        public Guid? OriginalAppoinment {  get; set; }
        public Guid? PaymentId { get; set; }
        public int AppointmentFee { get; set; }
        public string Note { get; set; } = null!;
        public int CyleCount { get; set; }

        public const string Checkup = "checkup";
        public const string Treatment = "treatment";

        public const string Booked = "booked";
        public const string Pending = "pending";
        public const string Finished = "finished";
        public const string Canceled = "canceled";
        public const string NotShowed = "no show";
    }
}
