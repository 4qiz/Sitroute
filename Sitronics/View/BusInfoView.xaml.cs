using LiveCharts;
using LiveCharts.Wpf;
using Sitronics.Models;
using Sitronics.Repositories;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sitronics.View
{
    /// <summary>
    /// Логика взаимодействия для BusInfoView.xaml
    /// </summary>
    public partial class BusInfoView : UserControl
    {
        List<Route> Routes = new();
        public BusInfoView()
        {
            InitializeComponent();

            //Bus currentBus = new();

            //currentBus = context.Buses.Include(b => b.Schedules).ThenInclude(s => s.IdBusStationNavigation)
            //    .Where(b => b.IdBus == 1).First();


            //numberBusTextBlock.Text = "Номер:" + currentBus.Number.ToUpper();
            //countPeopleTextBlock.Text = "Людей в автобусе:" + (currentBus.Schedules.Sum(s => s.PeopleCountBoardingBus) -
            //    currentBus.Schedules.Sum(s => s.PeopleCountGettingOffBus)).ToString();

            //ChartValues<int> peopleCountBoardingBus = new();
            //ChartValues<int> peopleCountGettingOffBus = new();
            //List<string> labels = new();
            //var shedules = currentBus.Schedules
            //    .Where(s => s.PeopleCountBoardingBus != null && s.PeopleCountGettingOffBus != null).ToList();
            //int step = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Math.Max(shedules.Max(s => s.PeopleCountBoardingBus).Value,
            //    shedules.Max(s => s.PeopleCountGettingOffBus).Value) / 10)));
            //foreach (var shedule in shedules)
            //{
            //    peopleCountBoardingBus.Add(shedule.PeopleCountBoardingBus ?? 0);
            //    peopleCountGettingOffBus.Add(shedule.PeopleCountGettingOffBus ?? 0);
            //    labels.Add(shedule.IdBusStationNavigation.Name);
            //}
            //peopleCountBoardingBusLineSeries.Values = peopleCountBoardingBus;
            //peopleCountGettingOffBusLineSeries.Values = peopleCountGettingOffBus;
            //xAxis.Labels = labels;
            //yAxis.Separator = new LiveCharts.Wpf.Separator() { Step = step };
        }

        public StackPanel GetBusInfoStackPanel(Bus bus)
        {
            var busInfoStackPanel = new StackPanel();
            var shedules = bus.Schedules
                .Where(s => s.PeopleCountBoardingBus != null && s.PeopleCountGettingOffBus != null && s.Time.Date == DateTime.Today.Date).ToList();
            busInfoStackPanel.Children.Add(new TextBlock()
            {
                Text = "Номер: " + bus.Number.ToUpper(),
                FontSize = 30,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))
            });
            if (shedules.Count > 0)
            {
                busInfoStackPanel.Children.Add(new TextBlock()
                {
                    Text = "Людей в автобусе: " + (bus.Schedules.Sum(s => s.PeopleCountBoardingBus) -
                        bus.Schedules.Sum(s => s.PeopleCountGettingOffBus)).ToString(),
                    FontSize = 20,
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))
                });
                ChartValues<int> peopleCountBoardingBus = new();
                ChartValues<int> peopleCountGettingOffBus = new();
                List<string> labels = new();
                foreach (var shedule in shedules)
                {
                    peopleCountBoardingBus.Add(shedule.PeopleCountBoardingBus ?? 0);
                    peopleCountGettingOffBus.Add(shedule.PeopleCountGettingOffBus ?? 0);
                    labels.Add(shedule.IdBusStationNavigation.Name);
                }
                var busCartesianChart = new CartesianChart()
                {
                    Height = 300
                };
                busCartesianChart.Series.Add(new LineSeries()
                {
                    Title = "Вошедшие в автобус",
                    LineSmoothness = 0,
                    Values = peopleCountBoardingBus
                });
                busCartesianChart.Series.Add(new LineSeries()
                {
                    Title = "Вышедшие из автобуса",
                    LineSmoothness = 0,
                    Values = peopleCountGettingOffBus
                });
                int step = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Math.Max(shedules.Max(s => s.PeopleCountBoardingBus).Value,
                    shedules.Max(s => s.PeopleCountGettingOffBus).Value) / 10)));
                busCartesianChart.AxisY.Add(new Axis
                {
                    MinValue = 0,
                    Title = "Количество людей",
                    Separator = new LiveCharts.Wpf.Separator() { Step = step }
                });
                busCartesianChart.AxisX.Add(new Axis
                {
                    Title = "Остановки",
                    Separator = new LiveCharts.Wpf.Separator() { Step = 1 },
                    Labels = labels
                });
                busInfoStackPanel.Children.Add(busCartesianChart);
            }
            else
                busInfoStackPanel.Children.Add(new TextBlock()
                {
                    Text = "Этот автобус еще не собрал статистику",
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))
                });
            return busInfoStackPanel;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
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

        private async Task LoadData()
        {
            Routes = await Connection.Client.GetFromJsonAsync<List<Route>>("/routesStats");
            routesStackPanel.Children.Clear();
            foreach (var route in Routes)
            {
                var busesStackPanel = new StackPanel();
                foreach (var bus in route.Buses)
                {
                    var busExpander = new Expander()
                    {
                        Header = bus.Number,
                        FontSize = 20,
                        Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                        Margin = new Thickness() { Left = 20 },
                        Content = GetBusInfoStackPanel(bus)
                    };
                    busesStackPanel.Children.Add(busExpander);
                }

                var routeExpander = new Expander()
                {
                    Header = route.Name,
                    FontSize = 30,
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                    Content = busesStackPanel
                };
                routesStackPanel.Children.Add(routeExpander);
            }
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
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
    }
}
