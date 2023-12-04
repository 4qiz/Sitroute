using Sitronics.Data;
using System.Windows.Controls;

namespace Sitronics.View
{
    /// <summary>
    /// Логика взаимодействия для StopInfoView.xaml
    /// </summary>
    public partial class StopInfoView : UserControl
    {
        public StopInfoView()
        {
            InitializeComponent();

            using (var context = new SitrouteDataContext())
            {
                var busStops = context.BusStations.Select(s => new
                {
                    s.Name,
                    s.PeopleCount
                }).ToList();
                BusStopsDataGrid.ItemsSource = busStops;

            }
        }
    }
}
