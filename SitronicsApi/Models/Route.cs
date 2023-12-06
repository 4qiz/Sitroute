using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SitronicsApi.Models;

public partial class Route
{
    public int IdRoute { get; set; }

    public string Name { get; set; } = null!;

    public bool IsBacked { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    [JsonIgnore]
    public virtual ICollection<Factor> Factors { get; set; } = new List<Factor>();

    public virtual ICollection<RouteByBusStation> RouteByBusStations { get; set; } = new List<RouteByBusStation>();

    public virtual ICollection<RouteHasBu> RouteHasBus { get; set; } = new List<RouteHasBu>();
}
