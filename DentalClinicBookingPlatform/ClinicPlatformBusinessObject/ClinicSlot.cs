using System;
using System.Collections.Generic;

namespace ClinicPlatformDatabaseObject;

public partial class ClinicSlot
{
    public Guid SlotId { get; set; }

    public int MaxCheckup { get; set; }

    public int MaxTreatment { get; set; }

    /// <summary>
    /// 0: Sunday 
    /// 1: Monday 
    /// 2: Tuesday 
    /// 3: Wednesday 
    /// 4: Thursday 
    /// 5: Friday 
    /// 6: Saturday
    /// </summary>
    public byte Weekday { get; set; }

    public int ClinicId { get; set; }

    public int TimeId { get; set; }

    public bool Status { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual Clinic Clinic { get; set; } = null!;

    public virtual Slot Time { get; set; } = null!;
}
