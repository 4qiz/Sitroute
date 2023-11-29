using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace Sitronics.Models;

public partial class Bus
{
    public int IdBus { get; set; }

    public bool IsBroken { get; set; }

    public string Number { get; set; } = null!;

    public Geometry? Location { get; set; }

    public int? IdRoute { get; set; }

    public int Charge { get; set; }

    public int? AverageChargeDrop { get; set; }

    public virtual Route? IdRouteNavigation { get; set; }

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public virtual ICollection<Driver> IdDrivers { get; set; } = new List<Driver>();
}
