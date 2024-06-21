using System;
using System.Collections.Generic;

namespace Repositories.Models;

public partial class MediaType
{
    public int TypeId { get; set; }

    public string TypeName { get; set; } = null!;

    public virtual ICollection<Medium> Media { get; set; } = new List<Medium>();
}
