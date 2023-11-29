using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using Microsoft.EntityFrameworkCore;
using Sitronics.Data;
using System.Diagnostics;
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
        public MapView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("sdfg");
        }

        private void AddStopButton_Click(object sender, RoutedEventArgs e)
        {
            var fm = new AddStopWindow();
            fm.Show();
        }

        private void MapView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                List<PointLatLng> points = new List<PointLatLng>();
                Random random = new();

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
                RoutingProvider routingProvider =
                mapView.MapProvider as RoutingProvider ?? GMapProviders.OpenStreetMap;
                mapView.SetPositionByKeywords("Архангельский Колледж Телекоммуникаций");
                using (var context = new SitrouteDataContext())
                {
                    var busStations = context.BusStations.ToList();
                    var buses = context.Buses.ToList();
                    var routes = context.Routes.Include(r => r.RouteByBusStations).ThenInclude(rp => rp.IdBusStationNavigation);
                    foreach (var dbroute in routes)
                    {
                        var routeColor = new SolidColorBrush(Color.FromArgb(255, (byte)random.Next(255), (byte)random.Next(255), (byte)random.Next(255)));
                        points.Clear();
                        foreach (var routePoint in dbroute.RouteByBusStations)
                        {
                            points.Add(new PointLatLng(routePoint.IdBusStationNavigation.Location.Coordinate.Y, routePoint.IdBusStationNavigation.Location.Coordinate.X));
                        }
                        AddRouteOnMap(points, routeColor, routingProvider);
                    }
                    foreach (var bus in buses)
                    {
                        var point = new PointLatLng(bus.Location.Coordinate.Y, bus.Location.Coordinate.X);
                        MapManager.MapManager.CreateMarker(point, ref mapView);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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
            fm.Show();
        }
    }
}
