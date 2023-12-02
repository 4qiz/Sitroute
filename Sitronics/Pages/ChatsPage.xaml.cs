using Microsoft.EntityFrameworkCore;
using Sitronics.Data;
using Sitronics.Models;
using Sitronics.Repositories;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sitronics.Pages
{
    /// <summary>
    /// Логика взаимодействия для ChatsPage.xaml
    /// </summary>
    public partial class ChatsPage : Page
    {
        List<Message> Messages;
        List<Driver> Drivers;

        public ChatsPage()
        {
            InitializeComponent();

            LoadData();

            Manager.MainTimer.Tick += new EventHandler(UpdateTimer_Tick);
        }
        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            chatsStackPanel.Children.Clear();
            using (var context = new SitrouteDataContext())
            {
                Messages = context.Messages
                    .Include(m => m.IdRecipientNavigation)
                    .Include(m => m.IdSenderNavigation)
                    .ThenInclude(u => u.Driver)
                    .Where(m => Connection.CurrentUser.IdUser == m.IdSender
                    || Connection.CurrentUser.IdUser == m.IdRecipient
                    || null == m.IdRecipient)
                    .OrderByDescending(m => m.Time)
                    .AsNoTracking()
                    .ToList();

                Drivers = context.Drivers.Include(d => d.IdDriverNavigation).AsNoTracking().ToList();
            }
            foreach (var message in Messages)
            {
                var driver = Drivers.FirstOrDefault(d => d.IdDriver == message.IdSender || d.IdDriver == message.IdRecipient);
                if (driver != null)
                {
                    Drivers.Remove(driver);
                    chatsStackPanel.Children.Add(GetChat(driver, message));
                }
            }
            foreach (var driver in Drivers)
            {
                chatsStackPanel.Children.Add(GetChat(driver));
            }
        }

        private static Button GetChat(Driver? driver, Message? message = null)
        {
            //var nameTextBlock = new TextBlock() 
            //{
            //    Text = $"{driver.IdDriverNavigation.SecondName} {driver.IdDriverNavigation.FirstName}" +
            //    $" {driver.IdDriverNavigation.Patronymic}"
            //};
            var chatTextBlock = new TextBlock
            {
                Text = $"{driver.IdDriverNavigation.SecondName} {driver.IdDriverNavigation.FirstName}" +
                $" {driver.IdDriverNavigation.Patronymic}"
            };
            if (message != null)
            {
                var today = DateTime.Now;
                chatTextBlock.Text += $"\t{(message.Time.Date == today.Date ? message.Time.ToString("HH:mm") :
                    message.Time.Year == today.Year ? message.Time.ToString("dd MMM") : message.Time.ToString("dd MMM yyyy"))}" +
                    $"\n{(message.IdRecipient == driver.IdDriver ? "Вы:" : "")} {message.Value}";
            }
            var chatItemStackPanel = new StackPanel() 
            {
                MaxHeight = 70,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin =new Thickness(5),
            };
            chatItemStackPanel.Children.Add(chatTextBlock);
            var chatButton = new Button {
                Content = chatItemStackPanel,
                
                Tag = driver.IdDriver,
                //Width = 400,
                //HorizontalAlignment = HorizontalAlignment.Left,
                //HorizontalContentAlignment = HorizontalAlignment.Left,
                //Background = new SolidColorBrush(Color.FromRgb(70, 69, 89)),
                //Foreground = new SolidColorBrush(Color.FromRgb(227, 224, 249)),
            };
            chatButton.Style = Application.Current.Resources["chatItemButton"] as Style;
            chatButton.Click += (object sender, RoutedEventArgs e) => { Manager.MainFrame.Navigate(new CurrentChatPage(driver));};
            return chatButton;
        }
    }
}
