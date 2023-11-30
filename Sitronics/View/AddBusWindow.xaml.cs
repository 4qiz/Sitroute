﻿using Sitronics.Data;
using Sitronics.Models;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Sitronics.View
{
    /// <summary>
    /// Логика взаимодействия для AddBusWindow.xaml
    /// </summary>
    public partial class AddBusWindow : Window
    {
        public AddBusWindow()
        {
            InitializeComponent();

            capacityTextBox.Text = "60";

            using (var context = new SitrouteDataContext())
            {
                routeComboBox.ItemsSource = context.Routes.ToList();
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

        private void AddBusButton_Click(object sender, RoutedEventArgs e)
        {
            using(var context = new SitrouteDataContext())
            {
                context.Buses.Add(new Bus()
                {
                    Number = numberBusTextBox.Text,
                    IdRoute = (routeComboBox.SelectedItem as Route).IdRoute
                });
                context.SaveChanges();
                MessageBox.Show("Автобус добавлен");
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
    }
}
