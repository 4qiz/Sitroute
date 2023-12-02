using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Sitronics.Models;

public partial class BusStation
{
    public int IdBusStation { get; set; }

    public int? PeopleCount { get; set; }

    public string Name { get; set; } = null!;

    [JsonIgnore]
    public Geometry Location { get; set; } = null!;

    [NotMapped]
    public string LocationForSerialization
    {
        get { return Location.ToString(); }
    }

    [JsonIgnore]
    public virtual ICollection<RouteByBusStation> RouteByBusStations { get; set; } = new List<RouteByBusStation>();

    [JsonIgnore]
    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
