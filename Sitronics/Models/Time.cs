using System;
using System.Collections.Generic;

namespace Sitronics.Models;

public partial class Time
{
    public int IdTime { get; set; }

    public DateTime Time1 { get; set; }

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
