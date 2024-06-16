using System;
using System.Collections.Generic;

namespace ClinicPlatformBusinessObject;

public partial class Service
{
    public int ServiceId { get; set; }

    public string ServiceName { get; set; } = null!;

    public virtual ICollection<ClinicService> ClinicServices { get; set; } = new List<ClinicService>();
}
