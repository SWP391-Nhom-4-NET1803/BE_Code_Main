using System;
using System.Collections.Generic;

namespace ClinicPlatformBusinessObject;

public partial class Token
{
    public Guid Id { get; set; }

    public string TokenValue { get; set; } = null!;

    public string Reason { get; set; } = null!;

    public DateTime CreationTime { get; set; }

    public bool Used { get; set; }

    public DateTime Expiration { get; set; }

    public int User { get; set; }

    public virtual User UserNavigation { get; set; } = null!;
}
