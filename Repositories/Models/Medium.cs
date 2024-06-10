using System;
using System.Collections.Generic;

namespace Repositories.Models;

public partial class Medium
{
    public Guid MediaId { get; set; }

    public int? MediaPath { get; set; }

    public DateTime CreatedDate { get; set; }

    public int TypeId { get; set; }

    public int CreatorId { get; set; }

    public virtual ICollection<Certification> Certifications { get; set; } = new List<Certification>();

    public virtual User Creator { get; set; } = null!;

    public virtual MediaType Type { get; set; } = null!;
}
