using System;
using System.Collections.Generic;

namespace ClinicPlatformDatabaseObject;

public partial class Customer
{
    public int Id { get; set; }

    public string? Insurance { get; set; }

    public DateOnly? Birthdate { get; set; }

    public string? Sex { get; set; }

    public int UserId { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual User User { get; set; } = null!;
}
