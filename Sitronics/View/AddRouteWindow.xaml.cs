using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using Sitronics.Data;
using Sitronics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Sitronics.View
{
    /// <summary>
    /// Логика взаимодействия для AddRouteWindow.xaml
    /// </summary>
    public partial class AddRouteWindow : Window
    {
        RoutingProvider routingProvider;

        public AddRouteWindow()
        {
            InitializeComponent();
            //CreateNewBusStopComboBox();
        }

        private void CreateNewBusStopComboBox()
        {
            using (var context = new SitrouteDataContext())
            {
                var busStations = context.BusStations.ToList();
                ComboBox comboBox = new ComboBox();
                comboBox.ItemsSource = busStations;
                comboBox.SelectedIndex = 0;
                comboBox.DisplayMemberPath = "Name";
                comboBox.Margin = new Thickness(0, 5, 0, 0);
                comboBoxesStackPanel.Children.Add(comboBox);
            }
        }

        private void MapView_Loaded(object sender, RoutedEventArgs e)
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
            routingProvider =
                mapView.MapProvider as RoutingProvider ?? GMapProviders.OpenStreetMap;
            mapView.SetPositionByKeywords("Архангельский Колледж Телекоммуникаций");
        }

        private void AddBusStopButton_Click(object sender, RoutedEventArgs e)
        {
            CreateNewBusStopComboBox();
        }

        private void SaveRouteButton_Click(object sender, RoutedEventArgs e)
        {
            MapRoute route;
            GMapRoute mapRoute;
            RouteByBusStation rbp;
            Models.Route dbroute = new();
            BusStation busStation;
            List<PointLatLng> points = new List<PointLatLng>();

            dbroute.Name = routeNameTextBox.Text;
            var comboBoxes = comboBoxesStackPanel.Children;
            foreach (ComboBox comboBox in comboBoxes)
            {
                busStation = (BusStation)comboBox.SelectedItem;
                rbp = new() { IdBusStation = busStation.IdBusStation };
                dbroute.RouteByBusStations.Add(rbp);
                points.Add(new PointLatLng(busStation.Location.Coordinate.Y, busStation.Location.Coordinate.X));
            }
            for (int i = 0; i < points.Count - 1; i++)
            {
                route = routingProvider.GetRoute(
                points[i], //start
                points[i + 1], //end
                false, //avoid highways 
                false, //walking mode\
                (int)mapView.Zoom);
                mapRoute = new GMapRoute(route.Points);
                mapView.Markers.Add(mapRoute);
            }
            using (var context = new SitrouteDataContext())
            {
                context.Routes.Add(dbroute);
                context.SaveChanges();
            }
        }


        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);
        private void pnlControlBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SendMessage(helper.Handle, 161, 2, 0);
        }

        private void pnlControlBar_MouseEnter(object sender, MouseEventArgs e)
        {
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal) WindowState = WindowState.Maximized;
            else WindowState = WindowState.Normal;
        }
    }
}
