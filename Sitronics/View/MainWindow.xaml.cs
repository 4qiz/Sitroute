﻿using Sitronics.Data;
using Sitronics.Repositories;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Sitronics.View
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Popup popup;

        public MainWindow()
        {
            InitializeComponent();
            if (Connection.CurrentUser?.Admin?.Role == "Руководитель")
            {
                ChatRadioButton.Visibility = Visibility.Collapsed;
                MessagesButton.Visibility = Visibility.Collapsed;
            }

            popUpView();

        }

        private void popUpView()
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = Connection.CurrentUser?.Admin?.Role?.ToString();
            textBlock.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1C1B1F"));
            textBlock.Padding = new Thickness(7);
            textBlock.Foreground = new SolidColorBrush(Colors.LightGray);
            popup = new Popup();
            popup.Child = textBlock;
            popup.Placement = PlacementMode.Mouse;
            popup.StaysOpen = false;
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

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                ExecuteRadioButtonCommandAndCheckIsChecked("InfoRadioButton");
            }

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.M)
            {

                ExecuteRadioButtonCommandAndCheckIsChecked("MapRadioButton");
            }

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.T)
            {

                ExecuteRadioButtonCommandAndCheckIsChecked("ScheduleRadioButton");
            }
            if (e.Key == Key.Escape)
            {
                Close();
            }

        }

        private void MessagesButton_Click(object sender, RoutedEventArgs e)
        {
            if (Connection.CurrentUser?.Admin?.Role != "Руководитель")
            {
                if (FindName("ChatRadioButton") is RadioButton radioButton)
                {
                    var command = radioButton.Command;
                    if (command != null && command.CanExecute(null))
                    {
                        command.Execute(null);
                        ChatRadioButton.IsChecked = true;
                    }
                }
            }
        }

        public void ExecuteRadioButtonCommandAndCheckIsChecked(string controlName)
        {
            // Find the control by name
            if (FindName(controlName) is RadioButton radioButton)
            {
                var command = radioButton.Command;
                if (command != null && command.CanExecute(null))
                {
                    command.Execute(null);
                    radioButton.IsChecked = true;
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите закрыть приложение?", "Подтверждение закрытия", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
            else 
                Environment.Exit(0);
        }

        
        private void MoreInfoUserButton_Click(object sender, RoutedEventArgs e)
        {
            
            popup.IsOpen = true;
        }
    }
}

