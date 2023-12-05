using Microsoft.EntityFrameworkCore;
using Sitronics.Data;
using Sitronics.Models;
using Sitronics.Repositories;
using System.Net.Http.Json;
using System.Windows;
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
        }

        private async Task LoadData()
        {
            using (var context = new SitrouteDataContext())
            {
                var route = await context.Routes
                .Include(r => r.RouteByBusStations)
                .ThenInclude(r => r.IdBusStationNavigation).ToListAsync();
                routeComboBox.ItemsSource = route;
                routeComboBox.SelectedIndex = 0;
                routeComboBox.DisplayMemberPath = "Name";

                stopComboBox.ItemsSource = ((Route)routeComboBox.SelectedItem).RouteByBusStations.ToList();
                stopComboBox.SelectedIndex = 0;
                stopComboBox.DisplayMemberPath = "IdBusStationNavigation.Name";

            }

        }

        private async Task LoadSchedule(Route selectedRoute, BusStation busStation)
        {
            List<Schedule> schedule = await Connection.Client.GetFromJsonAsync<List<Schedule>>(
                $"/schedule/{selectedRoute.IdRoute}/{busStation.IdBusStation}");
            if (schedule.Any())
            {
                scheduleDataGrid.ItemsSource = schedule.Where(s => s.IdBusStation == busStation.IdBusStation).OrderBy(s => s.Time).Select(s => new { Time = s.Time.ToString("t"), s.IdBus });
                scheduleDataGrid.Columns[0].Header = "Время";
            }



            // мб добавим на замену DataGrid
            /*var sch = schedule.Where(s => s.IdBusStation == busStation.IdBusStation).OrderBy(s => s.Time).Select(s => new { Time = s.Time.ToString("t"), s.IdBus });
            foreach (var i in sch)
            {
                ComboBoss.Text += $"{i.Time}   ";
            }*/

        }

        private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void RouteComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateBusStopsComboBox();
        }

        private void UpdateBusStopsComboBox()
        {
            stopComboBox.ItemsSource = ((Route)routeComboBox.SelectedItem).RouteByBusStations.ToList();
            stopComboBox.SelectedIndex = 0;
            stopComboBox.DisplayMemberPath = "IdBusStationNavigation.Name";
        }

        private async void LoadScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                scheduleDataGrid.ItemsSource = null;
                LoadingGif.Visibility = Visibility.Visible;
                loadScheduleButton.IsEnabled = false;
                RouteByBusStation routeByBusStation = (RouteByBusStation)stopComboBox.SelectedItem;
                Route route = (Route)routeComboBox.SelectedItem;
                if (routeByBusStation == null || route == null)
                    return;
                await LoadSchedule(route, routeByBusStation.IdBusStationNavigation);
                LoadingGif.Visibility = Visibility.Collapsed;
                loadScheduleButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                LoadingGif.Visibility = Visibility.Collapsed;
                loadScheduleButton.IsEnabled = true;
                MessageBox.Show(ex.Message);
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
