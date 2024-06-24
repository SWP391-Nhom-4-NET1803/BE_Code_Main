using System;
using System.Collections.Generic;

namespace ClinicPlatformDatabaseObject;

public partial class UserPanfoymentInfo
{
    public int UserId { get; set; }

    public string? PaymentNumber { get; set; }

    public virtual ICollection<Payment> PaymentRecieveNavigations { get; set; } = new List<Payment>();

    public virtual ICollection<Payment> PaymentSenderNavigations { get; set; } = new List<Payment>();

    public virtual User User { get; set; } = null!;
}
