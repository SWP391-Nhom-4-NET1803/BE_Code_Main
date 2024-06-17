using System;
using System.Collections.Generic;

namespace ClinicPlatformBusinessObject;

public partial class PaymentType
{
    public int TypeId { get; set; }

    public string TypeProvider { get; set; } = null!;

    public string TypeProviderSecret { get; set; } = null!;

    public string TypeProviderToken { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
