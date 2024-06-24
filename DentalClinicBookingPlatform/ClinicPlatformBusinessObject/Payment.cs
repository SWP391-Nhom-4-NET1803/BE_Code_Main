using System;
using System.Collections.Generic;

namespace ClinicPlatformDatabaseObject;

public partial class Payment
{
    public int Id { get; set; }

    public string TransactionId { get; set; } = null!;

    public string? Title { get; set; }

    public decimal Amount { get; set; }

    public DateTime CreationTime { get; set; }

    public string Provider { get; set; } = null!;

    public Guid AppointmentId { get; set; }

    public int Sender { get; set; }

    public int Recieve { get; set; }

    public string? Other { get; set; }

    public virtual Appointment Appointment { get; set; } = null!;

    public virtual UserPanfoymentInfo RecieveNavigation { get; set; } = null!;

    public virtual UserPanfoymentInfo SenderNavigation { get; set; } = null!;
}
