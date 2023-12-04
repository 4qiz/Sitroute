using Sitronics.Data;
using System.Windows.Controls;

namespace Sitronics.View
{
    /// <summary>
    /// Логика взаимодействия для StopInfoView.xaml
    /// </summary>
    public partial class StopInfoView : UserControl
    {
        public StopInfoView()
        {
            InitializeComponent();

            using (var context = new SitrouteDataContext())
            {
                BusStopsDataGrid.ItemsSource = context.BusStations.Select(s => new
                {
                    s.Name,
                    s.PeopleCount
                }).ToList();
                BusStopsDataGrid.Columns[0].Header = "Название";
                BusStopsDataGrid.Columns[1].Header = "Кол-во людей";
            }
        }
    }
}
