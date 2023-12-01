using Microsoft.EntityFrameworkCore;
using Sitronics.Data;
using Sitronics.Models;
using System.Windows.Controls;

namespace Sitronics.View
{
    /// <summary>
    /// Логика взаимодействия для ScheduleView.xaml
    /// </summary>
    public partial class ScheduleView : UserControl
    {
        public ScheduleView()
        {
            InitializeComponent();
            BusScheduleAlgorithm algorithm = new BusScheduleAlgorithm();
            DateTime startTime = DateTime.Parse("2022-01-02 08:00:00");
            DateTime endTime = DateTime.Parse("2022-01-02 22:00:00");
            using (var context = new SitrouteDataContext())
            {
                var route = context.Routes
                    .Include(r => r.RouteByBusStations)
                    .ThenInclude(r => r.IdBusStationNavigation)
                    .Include(r => r.Buses);
                var buses = route.FirstOrDefault().RouteByBusStations;
                List<Schedule> schedule = algorithm.GenerateBusSchedule(
                    startTime,
                    endTime,
                    5,
                    route.FirstOrDefault(r => r.IdRoute == 1).RouteByBusStations.ToList(),
                    route.FirstOrDefault(r => r.IdRoute == 1).Buses.ToList(),
                    "",
                    ""
                    );
                scheduleDataGrid.ItemsSource = schedule;//.OrderBy(s => s.IdBusStation).ThenBy(s => s.Time);
            }
        }
    }
}
