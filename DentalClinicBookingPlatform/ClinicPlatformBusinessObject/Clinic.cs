using System;
using System.Collections.Generic;

namespace ClinicPlatformBusinessObject;

public partial class Clinic
{
    public int ClinicId { get; set; }

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public TimeOnly OpenHour { get; set; }

    public TimeOnly CloseHour { get; set; }

    public string? Description { get; set; }

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public bool Status { get; set; }

    public int OwnerId { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<Certification> Certifications { get; set; } = new List<Certification>();

    public virtual ICollection<ClinicService> ClinicServices { get; set; } = new List<ClinicService>();

    public virtual ICollection<ClinicStaff> ClinicStaffs { get; set; } = new List<ClinicStaff>();

    public virtual User Owner { get; set; } = null!;

    public virtual ICollection<ScheduledSlot> ScheduledSlots { get; set; } = new List<ScheduledSlot>();
}
