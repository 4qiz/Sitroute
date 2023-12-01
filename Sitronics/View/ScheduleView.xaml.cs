﻿using Microsoft.EntityFrameworkCore;
using Sitronics.Data;
using Sitronics.Models;
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
            try
            {
                using (var context = new SitrouteDataContext())
                {
                    var route = context.Routes
                    .Include(r => r.RouteByBusStations)
                    .ThenInclude(r => r.IdBusStationNavigation);
                    routeComboBox.ItemsSource = route.ToList();
                    routeComboBox.SelectedIndex = 0;
                    routeComboBox.DisplayMemberPath = "Name";
                    
                    stopComboBox.ItemsSource = ((Route)routeComboBox.SelectedItem).RouteByBusStations.ToList();
                    stopComboBox.SelectedIndex = 0;
                    stopComboBox.DisplayMemberPath = "IdBusStationNavigation.Name";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private async Task LoadSchedule(Route selectedRoute, BusStation busStation)
        {
            BusScheduleAlgorithm algorithm = new BusScheduleAlgorithm();
            DateTime startTime = DateTime.Parse("2022-01-02 08:00:00");
            DateTime endTime = DateTime.Parse("2022-01-02 22:00:00");
            using (var context = new SitrouteDataContext())
            {
                var route = context.Routes
                    .Where(r => r.IdRoute == selectedRoute.IdRoute)
                    .Include(r => r.RouteByBusStations)
                    .ThenInclude(r => r.IdBusStationNavigation)
                    .Include(r => r.Buses)
                    .FirstOrDefault();
                var buses = route.RouteByBusStations;
                List<Schedule> schedule = await Task.Run(() => algorithm.GenerateBusSchedule(
                    startTime,
                    endTime,
                    selectedRoute.IdRoute,
                    route.RouteByBusStations.ToList(),
                    route.Buses.ToList(),
                    "",
                    ""
                    ));
                scheduleDataGrid.ItemsSource = schedule.Where(s => s.IdBusStation == busStation.IdBusStation).OrderBy(s => s.Time);//.OrderBy(s => s.IdBusStation).ThenBy(s => s.Time);
                //schedule2DataGrid.ItemsSource = schedule2.OrderByDescending(s => s.IdBusStation).ThenBy(s => s.Time); */

            }
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
            await LoadSchedule((Route)routeComboBox.SelectedItem, ((RouteByBusStation)stopComboBox.SelectedItem).IdBusStationNavigation);
        }
    }
}
