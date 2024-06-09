using System;
using System.Collections.Generic;

namespace Repositories.Models;

public partial class Message
{
    public Guid MessageId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime CreationDate { get; set; }

    public int Sender { get; set; }

    public int Receiver { get; set; }

    public virtual User ReceiverNavigation { get; set; } = null!;

    public virtual User SenderNavigation { get; set; } = null!;
}
