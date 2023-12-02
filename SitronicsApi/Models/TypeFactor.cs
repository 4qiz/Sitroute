using System;
using System.Collections.Generic;

namespace SitronicsApi.Models;

public partial class TypeFactor
{
    public int IdType { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Factor> Factors { get; set; } = new List<Factor>();
}
