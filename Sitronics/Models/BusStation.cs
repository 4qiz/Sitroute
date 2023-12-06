using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using NetTopologySuite.Geometries;
using Sitronics.Repositories;

namespace Sitronics.Models;

public partial class BusStation
{
    public int IdBusStation { get; set; }

    public int? PeopleCount { get; set; }

    public string Name { get; set; } = null!;

    [JsonIgnore]
    public Geometry Location { get; set; } = null!;

    [NotMapped]
    public string LocationForDeserialization
    {
        get
        {
            return LocationForSerialization;
        }
        set
        {
            Location = ConverterGeometry.GetPointByString(value);
        }
    }

    [NotMapped]
    public string LocationForSerialization
    {
        get { return Location?.ToString(); }
    }

    public virtual ICollection<RouteByBusStation> RouteByBusStations { get; set; } = new List<RouteByBusStation>();

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
