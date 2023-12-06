using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SitronicsApi.Models;

public partial class Schedule
{
    public int IdSchedule { get; set; }

    public int IdBus { get; set; }

    public int IdBusStation { get; set; }

    public DateTime Time { get; set; }

    public int? PeopleCountBoardingBus { get; set; }

    public int? PeopleCountGettingOffBus { get; set; }

    [JsonIgnore]
    public virtual Bus IdBusNavigation { get; set; } = null!;

    public virtual BusStation IdBusStationNavigation { get; set; } = null!;
}
