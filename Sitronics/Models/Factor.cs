using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace Sitronics.Models;

public partial class Factor
{
    public int IdFactor { get; set; }

    public decimal SpeedСoefficient { get; set; }

    public decimal? Length { get; set; }

    public Geometry? Location { get; set; }

    public int IdType { get; set; }

    public int IdRoute { get; set; }

    public virtual Route IdRouteNavigation { get; set; } = null!;

    public virtual TypeFactor IdTypeNavigation { get; set; } = null!;
}
