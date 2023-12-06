using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using Sitronics.Models;
using Sitronics.Repositories;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Sitronics.View
{
    /// <summary>
    /// Логика взаимодействия для MapView.xaml
    /// </summary>
    public partial class MapView : UserControl
    {
        List<BusStation> BusStations { get; set; }
        List<Bus> Buses { get; set; }
        List<Models.Route> Routes { get; set; }
        List<Factor> Factors { get; set; }
        List<Models.Route> ShowedRoutes { get; set; }

        public MapView()
        {
            InitializeComponent();

            Manager.MainTimer.Tick += new EventHandler(UpdateTimer_Tick);

            if (Connection.CurrentUser?.Admin?.Role == "Руководитель")
            {
                AddIncidentButton.Visibility = Visibility.Collapsed;
            }


        }

        private async void UpdateTimer_Tick(object sender, EventArgs e)
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

        private void AddBusButton_Click(object sender, RoutedEventArgs e)
        {
            var fm = new AddBusWindow();
            fm.ShowDialog();
        }

        private void AddStopButton_Click(object sender, RoutedEventArgs e)
        {
            var fm = new AddStopWindow();
            fm.ShowDialog();
        }

        private async void MapView_Loaded(object sender, RoutedEventArgs e)
        {
            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            // choose your provider here
            mapView.MapProvider = GMap.NET.MapProviders.OpenStreetMapProvider.Instance;
            mapView.MinZoom = 10;
            mapView.MaxZoom = 17;
            // whole world zoom
            mapView.Zoom = 14;
            mapView.ShowCenter = false;
            // lets the map use the mousewheel to zoom
            mapView.MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter;
            // lets the user drag the map
            mapView.CanDragMap = true;
            // lets the user drag the map with the left mouse button
            mapView.DragButton = MouseButton.Left;
            mapView.SetPositionByKeywords("Архангельский Колледж Телекоммуникаций");
            try
            {
                await LoadData();
                if (Routes == null) return;
                foreach (var route in Routes)
                {
                    var routeCheckBox = new CheckBox()
                    {
                        Content = route.Name,
                        IsChecked = true
                    };
                    routeCheckBox.Checked += async (object sender, RoutedEventArgs e) =>
                    {
                        ShowedRoutes.Add(route);
                        try
                        {
                            await LoadData();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    };
                    routeCheckBox.Unchecked += async (object sender, RoutedEventArgs e) =>
                    {
                        ShowedRoutes.Remove(route);
                        try
                        {
                            await LoadData();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }

                    };
                    checkBoxesStackPanel.Children.Add(routeCheckBox);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private async Task LoadData()
        {
            BusStations = await Connection.Client.GetFromJsonAsync<List<BusStation>>("/busStations");
            Buses = await Connection.Client.GetFromJsonAsync<List<Bus>>("/buses");
            Routes = await Connection.Client.GetFromJsonAsync<List<Models.Route>>("/routesByBusStations");

            List<PointLatLng> points = new List<PointLatLng>();
            RoutingProvider routingProvider =
            mapView.MapProvider as RoutingProvider ?? GMapProviders.OpenStreetMap;
            Random random = new();

            mapView.Markers.Clear();
            if (ShowedRoutes == null)
                ShowedRoutes = Routes;
            foreach (var busStation in BusStations)
            {
                var point = new PointLatLng(busStation.Location.Coordinate.Y, busStation.Location.Coordinate.X);
                MapManager.MapManager.CreateBusStationMarker(point, ref mapView, busStation);
            }

            foreach (var dbroute in ShowedRoutes)
            {
                var routeColor = new SolidColorBrush(Color.FromArgb(255, (byte)random.Next(255), (byte)random.Next(255), (byte)random.Next(255)));
                points.Clear();
                foreach (var routePoint in dbroute.RouteByBusStations.OrderBy(r => r.SerialNumberBusStation))
                {
                    points.Add(new PointLatLng(routePoint.IdBusStationNavigation.Location.Coordinate.Y, routePoint.IdBusStationNavigation.Location.Coordinate.X));
                }
                AddRouteOnMap(points, routeColor, routingProvider);
                foreach (var bus in Buses.Where(b => b.IdRoute == dbroute.IdRoute))
                {
                    var point = new PointLatLng(bus.Location.Coordinate.Y, bus.Location.Coordinate.X);
                    MapManager.MapManager.CreateBusMarker(point, ref mapView, bus);
                }
            }

            //foreach (var factor in Factors)
            //{
            //    var point = new PointLatLng(factor.Location.Coordinate.Y, factor.Location.Coordinate.X);
            //    MapManager.MapManager.CreateIncidentMarker(point, ref mapView, factor);
            //}
        }

        private void AddRouteOnMap(List<PointLatLng> points, SolidColorBrush routeColor, RoutingProvider routingProvider)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                var route = routingProvider.GetRoute(
                                        points[i], //start
                                        points[i + 1], //end
                                        false, //avoid highways 
                                        false, //walking mode
                                        (int)mapView.Zoom);
                Debug.WriteLine(route.Distance.ToString());
                var mapRoute = new GMapRoute(route.Points);
                mapRoute.Shape = new Path() { Stroke = routeColor, StrokeThickness = 4 };
                mapView.Markers.Add(mapRoute);
            }
        }

        private void AddRouteButton_Click(object sender, RoutedEventArgs e)
        {
            var fm = new AddRouteWindow();
            fm.ShowDialog();
        }

        private void AddIncidentButton_Click(object sender, RoutedEventArgs e)
        {
            var fm = new AddIncidentWindow();
            fm.ShowDialog();
        }

        private void NothingRoutesRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (checkBoxesStackPanel == null) return;
            foreach (CheckBox routeCheckBox in checkBoxesStackPanel.Children)
            {
                routeCheckBox.IsChecked = false;
            }
        }

        private void AllRoutesRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (checkBoxesStackPanel == null) return;
            foreach (CheckBox routeCheckBox in checkBoxesStackPanel.Children)
            {
                routeCheckBox.IsChecked = true;
            }
        }
    }
}
