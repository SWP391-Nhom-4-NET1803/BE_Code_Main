using System;
using System.Collections.Generic;

namespace Repositories.Models;

public partial class Slot
{
    public Guid SlotId { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public virtual ICollection<ScheduledSlot> ScheduledSlots { get; set; } = new List<ScheduledSlot>();
}
