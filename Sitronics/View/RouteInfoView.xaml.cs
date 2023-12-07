using Sitronics.Models;
using Sitronics.Repositories;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;

namespace Sitronics.View
{
    /// <summary>
    /// Логика взаимодействия для RouteInfoView.xaml
    /// </summary>
    public partial class RouteInfoView : UserControl
    {
        ObservableCollection<Route> Routes { get; set; } = new ObservableCollection<Route>();

        public RouteInfoView()
        {
            InitializeComponent();

            RoutesComboBox.ItemsSource = Routes;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            List<Route> routes = await Connection.Client.GetFromJsonAsync<List<Route>>("/routes");
            BusScheduleAlgorithm busScheduleAlgorithm = new BusScheduleAlgorithm();

            var modifiedRoutes = routes.AsParallel()
                .Select(r => new
                {
                    Route = r,
                    Profit = Math.Round(busScheduleAlgorithm.GetRouteProfitModifier(r.IdRoute),2),
                    AmountPeopleToday = busScheduleAlgorithm.GetPeopleOnRouteByDay(DateTime.Today, r.IdRoute),
                    RouteTime = busScheduleAlgorithm.GetIntervalInMinutesBetweenBusStations(r.IdRoute, r.RouteByBusStations.OrderBy(r => r.SerialNumberBusStation).First().IdBusStation,
                    r.RouteByBusStations.OrderBy(r => r.SerialNumberBusStation).Last().IdBusStation),
                    busStationCount = r.RouteByBusStations.Count
                })
                .OrderByDescending(r => r.Profit)
                .ToList();

            routesDataGrid.ItemsSource = modifiedRoutes.Select(mr => new { mr.Route.Name, mr.Profit, mr.AmountPeopleToday, mr.RouteTime, mr.busStationCount });
            routesDataGrid.Columns[0].Header = "Название маршрута";
            routesDataGrid.Columns[1].Header = "Релевантность";
            routesDataGrid.Columns[2].Header = "Пассажиропоток за сегодняшний день";
            routesDataGrid.Columns[3].Header = "Время в пути (минут)";
            routesDataGrid.Columns[4].Header = "Количество остановок";


            foreach (var route in routes)
            {
                if (Routes.Any(r => r.Name == route.Name))
                    continue;
                Routes.Add(route);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = RoutesComboBox.SelectedItem as Route;
            if (selectedItem != null)
            {
                int idRoute = selectedItem.IdRoute;
                await Connection.Client.DeleteAsync($"/route/{idRoute}");
                System.Windows.Forms.MessageBox.Show("Успешное удаление");
            }

        }
    }
}
