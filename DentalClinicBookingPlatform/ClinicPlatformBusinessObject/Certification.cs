using System;
using System.Collections.Generic;

namespace ClinicPlatformBusinessObject;

public partial class Certification
{
    public Guid CertificationId { get; set; }

    public string? Name { get; set; }

    public string? CertificationUrl { get; set; }

    public int ClinicId { get; set; }

    public Guid MediaId { get; set; }

    public virtual Clinic Clinic { get; set; } = null!;

    public virtual Medium Media { get; set; } = null!;
}
