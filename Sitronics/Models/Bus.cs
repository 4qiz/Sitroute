using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using NetTopologySuite.Geometries;
using Sitronics.Repositories;

namespace Sitronics.Models;

public partial class Bus
{
    public int IdBus { get; set; }

    public bool IsBroken { get; set; }

    public string Number { get; set; } = null!;

    [JsonIgnore]
    public Geometry? Location { get; set; }

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

    public int? IdRoute { get; set; }

    public int? Charge { get; set; }

    public int? AverageChargeDrop { get; set; }

    public int Сapacity { get; set; }

    public virtual ICollection<RouteHasBu> RouteHasBus { get; set; } = new List<RouteHasBu>();

    public virtual ICollection<Driver> IdDrivers { get; set; } = new List<Driver>();
}
