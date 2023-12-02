using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Sitronics.Repositories
{
    internal static class Manager
    {
        public static Frame MainFrame {  get; set; }

        public static DispatcherTimer MainTimer { get; set; } = new DispatcherTimer();
    }
}
