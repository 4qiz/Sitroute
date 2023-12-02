using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sitronics.Models;

public partial class Admin
{
    public int IdUser { get; set; }

    public string Role { get; set; } = null!;

    [JsonIgnore]
    public virtual User IdUserNavigation { get; set; } = null!;
}
