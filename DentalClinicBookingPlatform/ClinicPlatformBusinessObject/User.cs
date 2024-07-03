using System;
using System.Collections.Generic;

namespace ClinicPlatformDatabaseObject;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Salt { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Fullname { get; set; }

    public string? Phone { get; set; }

    public DateTime CreationTime { get; set; }

    public string Role { get; set; } = null!;

    public bool Active { get; set; }

    public bool Removed { get; set; }

    public virtual ICollection<Clinic> Clinics { get; set; } = new List<Clinic>();

    public virtual Customer? Customer { get; set; }

    public virtual Dentist? Dentist { get; set; }

    public virtual ICollection<Token> Tokens { get; set; } = new List<Token>();
}
