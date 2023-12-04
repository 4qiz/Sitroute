using Sitronics.Models;
using Sitronics.Repositories;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace Sitronics.View
{
    /// <summary>
    /// Логика взаимодействия для AddBusWindow.xaml
    /// </summary>
    public partial class AddBusWindow : Window
    {
        ObservableCollection<Route> Routes { get; set; } = new ObservableCollection<Route>();

        public AddBusWindow()
        {
            InitializeComponent();

            capacityTextBox.Text = "60";

            routeComboBox.ItemsSource = Routes;

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

        private async void AddBusButton_Click(object sender, RoutedEventArgs e)
        {
            var bus = new Bus()
            {
                Number = numberBusTextBox.Text,
                IdRoute = (routeComboBox.SelectedItem as Route).IdRoute
            };
            var response = await Connection.Client.PostAsJsonAsync("/bus", bus);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Автобус успешно добавлен");
            }
            else
            {
                MessageBox.Show("Кажется такой автобус уже есть");
            }
        }

        private void NumberBusTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CorrectInfoInTextBoxes();
        }

        private void CorrectInfoInTextBoxes()
        {
            var regex = new Regex(@"[а-я]\d{3}[а-я]{2}", RegexOptions.IgnoreCase);
            MatchCollection matches = regex.Matches(numberBusTextBox.Text);
            if (matches.Count > 0 && capacityTextBox.Text != "")
                addBusButton.IsEnabled = true;
            else
                addBusButton.IsEnabled = false;
        }

        private void CapacityTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CorrectInfoInTextBoxes();
        }

        private void CapacityTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
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
            foreach (var route in await Connection.Client.GetFromJsonAsync<List<Route>>("/routes"))
            {
                if (Routes.Any(r => r.Name == route.Name))
                    continue;
                Routes.Add(route);
            }
        }
    }
}
