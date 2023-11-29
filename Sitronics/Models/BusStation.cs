using System;
using System.Collections.Generic;

namespace Sitronics.Models;

public partial class BusStation
{
    public int IdBusStation { get; set; }

    public int? PeopleCount { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<RouteByBusStation> RouteByBusStations { get; set; } = new List<RouteByBusStation>();

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
