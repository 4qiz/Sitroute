using Sitronics.Data;
using Sitronics.Models;
using Sitronics.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
                    Profit = busScheduleAlgorithm.GetRouteProfitModifier(r.IdRoute)
                })
                .OrderByDescending(r => r.Profit)
                .ToList();

            routesDataGrid.ItemsSource = modifiedRoutes.Select(mr => new { mr.Route.IdRoute, mr.Route.Name, profit = mr.Profit });

            foreach (var route in await Connection.Client.GetFromJsonAsync<List<Route>>("/routes"))
            {
                if (Routes.Any(r => r.Name == route.Name))
                    continue;
                Routes.Add(route);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            int IdRoute = (RoutesComboBox.SelectedItem as Route).IdRoute;
            await Connection.Client.DeleteAsync($"/route/{IdRoute}");
        }
    }
}
