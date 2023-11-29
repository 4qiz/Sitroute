using LiveCharts;
using Microsoft.EntityFrameworkCore;
using Sitronics.Data;
using Sitronics.Models;
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
        public BusInfoView()
        {
            InitializeComponent();

            Bus currentBus = new();
            List<Route> routes = new();

            using (var context = new SitrouteDataContext())
            {
                routes = context.Routes.ToList();
                currentBus = context.Buses.Include(b => b.Schedules).ThenInclude(s => s.IdBusStationNavigation)
                    .Where(b => b.IdBus == 1).First();
            }
            foreach (var route in routes)
            {
                var busesStackPanel = new StackPanel();
                foreach (var item in route.Buses)
                {
                    var busExpander = new Expander()
                    {
                        Header = route.Name,
                        FontSize = 20,
                        Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                        Margin = new Thickness() { Left = 20},
                        
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

            numberBusTextBlock.Text = "Номер:" + currentBus.Number.ToUpper();
            countPeopleTextBlock.Text = "Людей в автобусе:" + (currentBus.Schedules.Sum(s => s.PeopleCountBoardingBus) -
                currentBus.Schedules.Sum(s => s.PeopleCountGettingOffBus)).ToString();

            ChartValues<int> peopleCountBoardingBus = new();
            ChartValues<int> peopleCountGettingOffBus = new();
            List<string> labels = new();
            var shedules = currentBus.Schedules
                .Where(s => s.PeopleCountBoardingBus != null && s.PeopleCountGettingOffBus != null).ToList();
            int step = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Math.Max(shedules.Max(s => s.PeopleCountBoardingBus).Value,
                shedules.Max(s => s.PeopleCountGettingOffBus).Value) / 10)));
            foreach (var shedule in shedules)
            {
                peopleCountBoardingBus.Add(shedule.PeopleCountBoardingBus ?? 0);
                peopleCountGettingOffBus.Add(shedule.PeopleCountGettingOffBus ?? 0);
                labels.Add(shedule.IdBusStationNavigation.Name);
            }
            peopleCountBoardingBusLineSeries.Values = peopleCountBoardingBus;
            peopleCountGettingOffBusLineSeries.Values = peopleCountGettingOffBus;
            xAxis.Labels = labels;
            yAxis.Separator = new LiveCharts.Wpf.Separator() { Step = step };
        }

        public StackPanel GetBusInfoStackPanel(Bus bus)
        {
            var busInfoStackPanel = new StackPanel();
            return busInfoStackPanel;
        }
    }
}
