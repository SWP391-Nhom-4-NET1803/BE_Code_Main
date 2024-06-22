using System;
using System.Collections.Generic;

namespace ClinicPlatformBusinessObject;

public partial class Slot
{
    public int Id { get; set; }

    public TimeOnly Start { get; set; }

    public TimeOnly End { get; set; }

    public virtual ICollection<ClinicSlot> ClinicSlots { get; set; } = new List<ClinicSlot>();
}
