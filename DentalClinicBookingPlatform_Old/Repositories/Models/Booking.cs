using System;
using System.Collections.Generic;

namespace Repositories.Models;

public partial class Booking
{
    public Guid BookId { get; set; }

    public DateOnly AppointmentDate { get; set; }

    public int CustomerId { get; set; }

    public int ClinicId { get; set; }

    public int DentistId { get; set; }

    public Guid ScheduleSlotId { get; set; }

    public DateTime CreationDate { get; set; }

    public bool Status { get; set; }

    public string BookingType { get; set; } = null!;

    public Guid? BookingServiceId { get; set; }

    public virtual ClinicService? BookingService { get; set; }

    public virtual Clinic Clinic { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;

    public virtual ClinicStaff Dentist { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ScheduledSlot ScheduleSlot { get; set; } = null!;
}
