using GMap.NET.MapProviders;
using GMap.NET;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using GMap.NET.WindowsPresentation;
using System.Runtime.InteropServices;

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
        }

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

        private void AddIncident_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mapView_Loaded(object sender, RoutedEventArgs e)
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

        private void mapView_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            int x = Convert.ToInt32(e.GetPosition(mapView).X);
            int y = Convert.ToInt32(e.GetPosition(mapView).Y);
        }
    }

}
