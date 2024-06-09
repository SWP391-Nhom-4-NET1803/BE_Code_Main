using System;
using System.Collections.Generic;

namespace Repositories.Models;

public partial class Certification
{
    public Guid CertificationId { get; set; }

    public string? Name { get; set; }

    public string? CertificationUrl { get; set; }

    public int ClinicId { get; set; }

    public virtual Clinic Clinic { get; set; } = null!;

    public virtual ICollection<Medium> Media { get; set; } = new List<Medium>();
}
