using System;
using System.Collections.Generic;

namespace Sitronics.Models;

public partial class Message
{
    public int IdMessage { get; set; }

    public string Value { get; set; } = null!;

    public int IdSender { get; set; }

    public int? IdRecipient { get; set; }

    public DateTime Time { get; set; }

    public virtual User? IdRecipientNavigation { get; set; }

    public virtual User IdSenderNavigation { get; set; } = null!;
}
