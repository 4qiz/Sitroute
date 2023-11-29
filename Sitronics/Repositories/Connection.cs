using Sitronics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitronics.Repositories
{
    public static class Connection
    {
        public static User CurrentUser { get; set; } = null;
    }
}
