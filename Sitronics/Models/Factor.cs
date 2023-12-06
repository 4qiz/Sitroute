using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using NetTopologySuite.Geometries;
using Sitronics.Repositories;

namespace Sitronics.Models;

public partial class Factor
{
    public int IdFactor { get; set; }

    public decimal SpeedСoefficient { get; set; }

    public decimal? Radius { get; set; }


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


    public int IdType { get; set; }

    public int IdRoute { get; set; }

    public virtual Route IdRouteNavigation { get; set; } = null!;

    public virtual TypeFactor IdTypeNavigation { get; set; } = null!;
}
