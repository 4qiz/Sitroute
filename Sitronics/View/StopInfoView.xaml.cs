using Sitronics.Data;
using Sitronics.Repositories;
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
        }

        private async Task LoadData()
        {
            await using (var context = new SitrouteDataContext())
            {
                var busStops = await Task.Run(() => context.BusStations
                    .Select(s => new
                {
                    s.Name,
                    s.PeopleCount
                }).ToList());
                BusStopsDataGrid.ItemsSource = busStops;
            }
        }

        private async void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await LoadData();
        }
    }
}
