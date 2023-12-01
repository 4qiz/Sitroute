using Microsoft.EntityFrameworkCore;
using Sitronics.Data;
using Sitronics.Models;
using Sitronics.Repositories;
using System.Windows;
using System.Windows.Controls;

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

            System.Windows.Threading.DispatcherTimer updateTimer = new System.Windows.Threading.DispatcherTimer();
            updateTimer.Tick += new EventHandler(UpdateTimer_Tick);
            updateTimer.Interval = new TimeSpan(0, 0, 30);
            updateTimer.Start();
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
                    $"\n{(message.IdRecipient == driver.IdDriver ? "Вы:" : "")}{message.Value}";
            }
            var chatButton = new Button { Content = chatTextBlock, Style = null, Tag = driver.IdDriver };
            chatButton.Click += (object sender, RoutedEventArgs e) => { Manager.MainFrame.Navigate(new CurrentChatPage(driver));};
            return chatButton;
        }
    }
}
