using System;
using System.Collections.Generic;

namespace ClinicPlatformDatabaseObject;

public partial class Appointment
{
    public Guid Id { get; set; }

    public string AppointmentType { get; set; } = null!;

    public DateOnly Date { get; set; }

    public Guid SlotId { get; set; }

    public int CustomerId { get; set; }

    public int DentistId { get; set; }

    public int ClinicId { get; set; }

    public string DentistNote { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int CycleCount { get; set; }

    public Guid? OriginalAppointment { get; set; }

    public int PriceFinal { get; set; }

    public DateTime CreationTime { get; set; }

    public virtual BookedService? BookedService { get; set; }

    public virtual Clinic Clinic { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;

    public virtual Dentist Dentist { get; set; } = null!;

    public virtual ICollection<Appointment> InverseOriginalAppointmentNavigation { get; set; } = new List<Appointment>();

    public virtual Appointment? OriginalAppointmentNavigation { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ClinicSlot Slot { get; set; } = null!;
}
