using Sitronics.Data;
using Sitronics.Models;
using Sitronics.Pages;
using Sitronics.Repositories;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sitronics.View
{
    /// <summary>
    /// Логика взаимодействия для ChatView.xaml
    /// </summary>
    public partial class ChatView : UserControl
    {
        Driver currentDriver = new Driver()
        {
            IdDriver = 1
        };
        public ChatView()
        {
            InitializeComponent();

            Manager.MainFrame = contentFrame;
            Manager.MainFrame.Navigate(new ChatsPage());
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.GoBack();
        }

        private void ContentFrame_ContentRendered(object sender, System.EventArgs e)
        {
            if (Manager.MainFrame.CanGoBack)
                backButton.Visibility = Visibility.Visible;
            else
                backButton.Visibility = Visibility.Collapsed;
        }
    }
}
