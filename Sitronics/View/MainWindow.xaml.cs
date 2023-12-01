using Sitronics.Repositories;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

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
            if (Connection.CurrentUser?.Admin?.Role == "Руководитель")
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

        private void ClockButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("control.exe", "timedate.cpl,,0");
        }
    }
}

