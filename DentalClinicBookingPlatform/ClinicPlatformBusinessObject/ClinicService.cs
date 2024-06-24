using System;
using System.Collections.Generic;

namespace ClinicPlatformDatabaseObject;

public partial class ClinicService
{
    public Guid Id { get; set; }

    public string? CustomName { get; set; }

    public string Description { get; set; } = null!;

    public int Price { get; set; }

    public int ClinicId { get; set; }

    public int CategoryId { get; set; }

    public bool Available { get; set; }

    public bool Removed { get; set; }

    public virtual ICollection<BookedService> BookedServices { get; set; } = new List<BookedService>();

    public virtual ServiceCategory Category { get; set; } = null!;

    public virtual Clinic Clinic { get; set; } = null!;
}
