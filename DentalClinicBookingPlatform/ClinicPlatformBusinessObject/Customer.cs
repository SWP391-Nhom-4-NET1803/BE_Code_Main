using System;
using System.Collections.Generic;

namespace ClinicPlatformBusinessObject;

public partial class Customer
{
    public int Id { get; set; }

    public string? Fullname { get; set; }

    public string? Insurance { get; set; }

    public string? Phone { get; set; }

    public DateOnly? Birthdate { get; set; }

    public string? Sex { get; set; }

    public bool Removed { get; set; }

    public int UserId { get; set; }

    public int? CustomerEmail { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual User User { get; set; } = null!;
}
