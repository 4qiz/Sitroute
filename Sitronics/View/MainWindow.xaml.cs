using FontAwesome.Sharp;
using Microsoft.Identity.Client;
using Sitronics.Repositories;
using Sitronics.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace Sitronics.View
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if(Connection.CurrentUser?.Admin?.Role == "Руководитель")
                ChatRadioButton.Visibility = Visibility.Collapsed;
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
            Application.Current.Shutdown();
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
            if(e.Key == Key.F1)
            {
                if (FindName("InfoRadioButton") is RadioButton radioButton)
                {
                    var command = radioButton.Command;
                    if (command != null && command.CanExecute(null))
                    {
                        command.Execute(null);
                        InfoRadioButton.IsChecked = true;
                    }
                }
            }

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.M)
            {
                if (FindName("MapRadioButton") is RadioButton radioButton)
                {
                    var command = radioButton.Command;
                    if (command != null && command.CanExecute(null))
                    {
                        command.Execute(null);
                        MapRadioButton.IsChecked = true;
                    }
                }
            }

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.T)
            {
                if (FindName("ScheduleRadioButton") is RadioButton radioButton)
                {
                    var command = radioButton.Command;
                    if (command != null && command.CanExecute(null))
                    {
                        command.Execute(null);
                        ScheduleRadioButton.IsChecked = true;
                    }
                }
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
    }
}

