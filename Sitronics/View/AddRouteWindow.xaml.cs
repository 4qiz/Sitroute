using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using Sitronics.Models;
using Sitronics.Repositories;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Sitronics.View
{
    /// <summary>
    /// Логика взаимодействия для AddRouteWindow.xaml
    /// </summary>
    public partial class AddRouteWindow : Window
    {
        RoutingProvider routingProvider;
        int countBusStation = 0;

        ObservableCollection<BusStation> BusStations = new ObservableCollection<BusStation>();

        public AddRouteWindow()
        {
            InitializeComponent();
            //CreateNewBusStopComboBox();
            Manager.MainTimer.Tick += new EventHandler(UpdateTimer_Tick);

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

        private async void CreateNewBusStopComboBox()
        {
            try
            {
                TextBox timeTextBox = new TextBox();
                timeTextBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1C1B1F"));
                timeTextBox.BorderBrush = new SolidColorBrush(Colors.LightGray);
                timeTextBox.Foreground = new SolidColorBrush(Colors.LightGray);
                timeTextBox.BorderThickness = new Thickness(1);
                timeTextBox.Width = 55;
                timeTextBox.Height = 26;
                timeTextBox.FontSize = 18;
                timeTextBox.Margin = new Thickness(0, 5, 15, 0);
                timeTextBox.VerticalAlignment= VerticalAlignment.Center;
                timeTextBox.MaxLength = 5;
                timeTextBox.Text = "08:00";
                timeTextBox.PreviewTextInput += TimeTextBox_PreviewTextInput;
                timeTextBox.TextChanged += TimeTextBox_TextChanged;


                StackPanel BusNTimePanel = new StackPanel();
                BusNTimePanel.Orientation = Orientation.Horizontal;
                BusNTimePanel.Margin = new Thickness(0, 0, 0, 5);

                await LoadData();
                ComboBox comboBox = new ComboBox();
                comboBox.ItemsSource = BusStations;
                comboBox.SelectedIndex = 0;
                comboBox.DisplayMemberPath = "Name";
                comboBox.Margin = new Thickness(0, 5, 0, 0);
                comboBox.SelectionChanged += ComboBox_SelectionChanged;

                BusNTimePanel.Children.Add(timeTextBox);
                BusNTimePanel.Children.Add(comboBox);
                BusStopsStackPanel.Children.Add(BusNTimePanel);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void TimeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox timeTextBox = (TextBox)sender;

            if (timeTextBox.Text.Length >= 3)
            {
                timeTextBox.Text = timeTextBox.Text.Remove(2, 1).Insert(2, ":");
                timeTextBox.SelectionStart = timeTextBox.Text.Length; // move cursor to the end
            }
        }

        private void TimeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!(Char.IsDigit(e.Text[0]) || e.Text[0].Equals(':')))
            {
                e.Handled = true;
            }
        }

        private async Task LoadData()
        {
            foreach (var busStation in await Connection.Client.GetFromJsonAsync<List<BusStation>>("/busStations"))
            {
                if (BusStations.Any(r => r.Name == busStation.Name))
                    continue;
                BusStations.Add(busStation);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mapView.Markers.Clear();
            Models.Route dbroute = AddBusPointToMap();
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
            try
            {
                CreateNewBusStopComboBox();
                CheckEnabled();
                mapView.Markers.Clear();
                Models.Route dbroute = AddBusPointToMap();
                countBusStation++;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void SaveRouteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Models.Route dbroute = AddBusPointToMap();
                var response = await Connection.Client.PostAsJsonAsync("/route", dbroute);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Маршрут успешно добавлен");
                }
                else
                {
                    MessageBox.Show("Кажется такой маршрут уже есть");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private Models.Route AddBusPointToMap()
        {
            MapRoute route;
            GMapRoute mapRoute;
            RouteByBusStation rbp;
            Models.Route dbroute = new();
            BusStation busStation;
            List<PointLatLng> points = new List<PointLatLng>();

            dbroute.Name = routeNameTextBox.Text;
            if (!DateTime.TryParse(startTimeTextBox.Text, out DateTime startTime))
            {
                throw new Exception("Неверное время начала маршрута");
            }
            if (!DateTime.TryParse(startTimeTextBox.Text, out DateTime endTime))
            {
                throw new Exception("Неверное время конца маршрута");
            }

            dbroute.StartTime = startTime;
            dbroute.EndTime = endTime;
            var serialNumberBusStation = 1;

            foreach (StackPanel stackPanel in BusStopsStackPanel.Children)
            {
                TextBox stopTimeTextBox = stackPanel.Children[0] as TextBox;
                ComboBox comboBox = stackPanel.Children[1] as ComboBox;

                if (!DateTime.TryParse(stopTimeTextBox.Text, out DateTime arrivalTime))
                {
                    throw new Exception("Неверное время прибытия на остановку");
                }

                busStation = (BusStation)comboBox.SelectedItem;
                rbp = new() { IdBusStation = busStation.IdBusStation, 
                    SerialNumberBusStation = serialNumberBusStation, 
                    StandardArrivalTime = arrivalTime
                };
                dbroute.RouteByBusStations.Add(rbp);
                points.Add(new PointLatLng(busStation.Location.Coordinate.Y, busStation.Location.Coordinate.X));
                serialNumberBusStation++;
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
            dbroute.IsBacked = isBackedCheckBox.IsChecked ?? false;

            return dbroute;

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

        private void RouteNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckEnabled();
        }

        private void CheckEnabled()
        {
            saveRouteButton.IsEnabled = routeNameTextBox.Text.Length > 0 && countBusStation > 1;
        }
    }
}
