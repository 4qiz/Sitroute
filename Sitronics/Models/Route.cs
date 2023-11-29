using System;
using System.Collections.Generic;

namespace Sitronics.Models;

public partial class Route
{
    public int IdRoute { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Bus> Buses { get; set; } = new List<Bus>();

    public virtual ICollection<Factor> Factors { get; set; } = new List<Factor>();

    public virtual ICollection<RouteByBusStation> RouteByBusStations { get; set; } = new List<RouteByBusStation>();
}
