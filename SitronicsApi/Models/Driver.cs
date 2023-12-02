using System;
using System.Collections.Generic;

namespace SitronicsApi.Models;

public partial class Driver
{
    public int IdDriver { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public virtual User IdDriverNavigation { get; set; } = null!;

    public virtual ICollection<Bus> IdBus { get; set; } = new List<Bus>();
}
