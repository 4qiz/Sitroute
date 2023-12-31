﻿using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using Sitronics.Data;
using Sitronics.Models;
using Sitronics.Repositories;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Sitronics.View
{
    /// <summary>
    /// Логика взаимодействия для AddStopWindow.xaml
    /// </summary>
    public partial class AddStopWindow : Window
    {
        PointLatLng point;
        GMapMarker lastMarker;

        public AddStopWindow()
        {
            InitializeComponent();
        }

        private async void MapView_Loaded(object sender, RoutedEventArgs e)
        {
            try
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

                List<BusStation> BusStations = await Connection.Client.GetFromJsonAsync<List<BusStation>>("/busStations");
                foreach (var busStation in BusStations)
                {
                    var point = new PointLatLng(busStation.Location.Coordinate.Y, busStation.Location.Coordinate.X);
                    MapManager.MapManager.CreateBusStationMarker(point, ref mapView, busStation);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void SaveBusStopButton_Click(object sender, RoutedEventArgs e)
        {
            var busStation = new BusStation
            {
                Location = new NetTopologySuite.Geometries.Point(point.Lng, point.Lat) { SRID = 4326 },
                Name = busStopNameTextBox.Text,
            };
            try
            {

                using (var context = new SitrouteDataContext())
                {
                    await context.BusStations.AddAsync(busStation);
                    await context.SaveChangesAsync();
                    MessageBox.Show("Автобусная остановка успешно добавлена");
                    MapManager.MapManager.CreateBusStationMarker(point, ref mapView, busStation);
                }
            }
            catch
            {
                MessageBox.Show("Автобусная остановка не была добавлена");
            }
        }

        private void MapView_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            int x = Convert.ToInt32(e.GetPosition(mapView).X);
            int y = Convert.ToInt32(e.GetPosition(mapView).Y);
            point = mapView.FromLocalToLatLng(x, y);
            lastMarker = MapManager.MapManager.CreateMarker(point, ref mapView, lastMarker: lastMarker, fillColor: Brushes.Red);
        }

        private void SearchPlaceButton_Click(object sender, RoutedEventArgs e)
        {
            mapView.SetPositionByKeywords(AddressTextBox.Text);
        }



        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);
        private void PnlControlBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SendMessage(helper.Handle, 161, 2, 0);
        }

        private void PnlControlBar_MouseEnter(object sender, MouseEventArgs e)
        {
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal) WindowState = WindowState.Maximized;
            else WindowState = WindowState.Normal;
        }
    }
}
