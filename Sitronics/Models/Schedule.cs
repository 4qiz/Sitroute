using System;
using System.Collections.Generic;

namespace Sitronics.Models;

public partial class Schedule
{
    public int IdBus { get; set; }

    public int IdBusStation { get; set; }

    public DateTime Time { get; set; }

    public int? PeopleCountBoardingBus { get; set; }

    public int? PeopleCountGettingOffBus { get; set; }

    public virtual Bus IdBusNavigation { get; set; } = null!;

    public virtual BusStation IdBusStationNavigation { get; set; } = null!;
}
