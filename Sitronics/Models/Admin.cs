using System;
using System.Collections.Generic;

namespace Sitronics.Models;

public partial class Admin
{
    public int IdUser { get; set; }

    public string Role { get; set; } = null!;

    public virtual User IdUserNavigation { get; set; } = null!;
}
