using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using Sitronics.Data;
using Sitronics.Models;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Sitronics.View
{
    /// <summary>
    /// Логика взаимодействия для AddIncidentWindow.xaml
    /// </summary>
    public partial class AddIncidentWindow : Window
    {
        public AddIncidentWindow()
        {
            InitializeComponent();
            using (var context = new SitrouteDataContext())
            {
                var typeFactors = context.TypeFactors;
                var routes = context.Routes;
                incidentsComboBox.ItemsSource = typeFactors.ToList();
                incidentsComboBox.DisplayMemberPath = "Name";
                routeComboBox.ItemsSource = routes.ToList();
                routeComboBox.DisplayMemberPath = "Name";
            }
        }
        PointLatLng point;
        GMapMarker lastMarker;

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal) WindowState = WindowState.Maximized;
            else WindowState = WindowState.Normal;
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
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

        private async void AddIncident_Click(object sender, RoutedEventArgs e)
        {
            var incident = new Factor();
            incident.Location = new NetTopologySuite.Geometries.Point(point.Lng, point.Lat) { SRID = 4326 };
            var type = (TypeFactor)incidentsComboBox.SelectedItem;
            incident.IdType = type.IdType;
            var route = (Models.Route)routeComboBox.SelectedItem;
            incident.IdRoute = route.IdRoute;
            try
            {
                using (var context = new SitrouteDataContext())
                {
                    await context.Factors.AddAsync(incident);
                    await context.SaveChangesAsync();
                    MessageBox.Show("Автобусная остановка успешно добавлена");
                    MapManager.MapManager.CreateIncidentMarker(point, ref mapView, incident);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
            RoutingProvider routingProvider =
            mapView.MapProvider as RoutingProvider ?? GMapProviders.OpenStreetMap;
            mapView.SetPositionByKeywords("Архангельский Колледж Телекоммуникаций");
        }

        private void MapView_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            int x = Convert.ToInt32(e.GetPosition(mapView).X);
            int y = Convert.ToInt32(e.GetPosition(mapView).Y);
            point = mapView.FromLocalToLatLng(x, y);
            lastMarker = MapManager.MapManager.CreateMarker(point, ref mapView, lastMarker: lastMarker, fillColor: Brushes.Red);
        }
    }

}
