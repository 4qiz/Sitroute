using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SitronicsApi.Models;

public partial class Driver
{
    public int IdDriver { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public virtual User IdDriverNavigation { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Bus> IdBus { get; set; } = new List<Bus>();
}
