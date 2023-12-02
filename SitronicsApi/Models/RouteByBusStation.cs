using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SitronicsApi.Models;

public partial class RouteByBusStation
{
    public int IdRoute { get; set; }

    public int IdBusStation { get; set; }

    public int SerialNumberBusStation { get; set; }

    public virtual BusStation IdBusStationNavigation { get; set; } = null!;

    [JsonIgnore]
    public virtual Route IdRouteNavigation { get; set; } = null!;
}
