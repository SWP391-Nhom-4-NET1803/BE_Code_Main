using System;
using System.Collections.Generic;

namespace Repositories.Models;

public partial class ScheduledSlot
{
    public Guid ScheduleSlotId { get; set; }

    public int MaxAppointments { get; set; }

    public byte DateOfWeek { get; set; }

    public int ClinicId { get; set; }

    public Guid SlotId { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual Clinic Clinic { get; set; } = null!;

    public virtual Slot Slot { get; set; } = null!;
}
