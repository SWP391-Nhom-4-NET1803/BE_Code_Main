using System;
using System.Collections.Generic;

namespace ClinicPlatformBusinessObject;

public partial class BookedService
{
    public Guid AppointmentId { get; set; }

    public Guid ServiceId { get; set; }

    public int Price { get; set; }

    public virtual Appointment Appointment { get; set; } = null!;

    public virtual ClinicService Service { get; set; } = null!;
}
