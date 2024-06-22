using System;
using System.Collections.Generic;

namespace ClinicPlatformBusinessObject;

public partial class Payment
{
    public int Id { get; set; }

    public int? TransactionId { get; set; }

    public decimal Amount { get; set; }

    public int? Title { get; set; }

    public DateOnly Expiration { get; set; }

    public DateTime CreationTime { get; set; }

    public bool Status { get; set; }

    public int Creator { get; set; }

    public Guid AppointmentId { get; set; }

    public int TypeId { get; set; }

    public string Provider { get; set; } = null!;

    public virtual Appointment Appointment { get; set; } = null!;

    public virtual User CreatorNavigation { get; set; } = null!;
}
