using System;
using System.Collections.Generic;

namespace Sitronics.Models;

public partial class RouteByBusStation
{
    public int IdRoute { get; set; }

    public int IdBusStation { get; set; }

    public int SerialNumberBusStation { get; set; }

    public virtual BusStation IdBusStationNavigation { get; set; } = null!;

    public virtual Route IdRouteNavigation { get; set; } = null!;
}
