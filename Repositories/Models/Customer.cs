using System;
using System.Collections.Generic;

namespace Repositories.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string? Sex { get; set; }

    public DateOnly? BirthDate { get; set; }

    public string? Insurance { get; set; }

    public int UserId { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual User User { get; set; } = null!;
}
