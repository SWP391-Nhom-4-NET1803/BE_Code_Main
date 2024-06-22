using System;
using System.Collections.Generic;

namespace ClinicPlatformBusinessObject;

public partial class Clinic
{
    public int ClinicId { get; set; }

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public TimeOnly OpenHour { get; set; }

    public TimeOnly CloseHour { get; set; }

    public bool Working { get; set; }

    public string Status { get; set; } = null!;

    public int OwnerId { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<ClinicService> ClinicServices { get; set; } = new List<ClinicService>();

    public virtual ICollection<ClinicSlot> ClinicSlots { get; set; } = new List<ClinicSlot>();

    public virtual ICollection<Dentist> Dentists { get; set; } = new List<Dentist>();

    public virtual User Owner { get; set; } = null!;
}
