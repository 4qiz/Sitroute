using System;
using System.Collections.Generic;

namespace Sitronics.Models;

public partial class RouteHasBu
{
    public int IdRoute { get; set; }

    public int IdBus { get; set; }

    public virtual Bus IdBusNavigation { get; set; } = null!;

    public virtual Route IdRouteNavigation { get; set; } = null!;

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
