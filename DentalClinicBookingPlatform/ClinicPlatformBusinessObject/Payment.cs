using System;
using System.Collections.Generic;

namespace ClinicPlatformDatabaseObject;

public partial class Payment
{
    public int Id { get; set; }

    public string TransactionId { get; set; } = null!;

    public string Info { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime ExpirationTime { get; set; }

    public string Provider { get; set; } = null!;

    public Guid AppointmentId { get; set; }

    public string Status { get; set; } = null!;

    public virtual Appointment Appointment { get; set; } = null!;
}
