using System;
using System.Collections.Generic;

namespace ClinicPlatformBusinessObject;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Salt { get; set; } = null!;

    public DateTime CreationTime { get; set; }

    public string Role { get; set; } = null!;

    public bool Active { get; set; }

    public bool Removed { get; set; }

    public virtual ICollection<Clinic> Clinics { get; set; } = new List<Clinic>();

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    public virtual Dentist? Dentist { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Token> Tokens { get; set; } = new List<Token>();
}
