using System;
using System.Collections.Generic;

namespace ClinicPlatformBusinessObject;

public partial class ClinicStaff
{
    public int StaffId { get; set; }

    public bool IsOwner { get; set; }

    public int UserId { get; set; }

    public int? ClinicId { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual Clinic? Clinic { get; set; }

    public virtual User User { get; set; } = null!;
}
