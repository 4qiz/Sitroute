using Sitronics.Models;
using Sitronics.Repositories;
using System.Net.Http.Json;
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
        private async void UpdateTimer_Tick(object sender, EventArgs e)
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            chatsStackPanel.Children.Clear();
            Messages = await Connection.Client.GetFromJsonAsync<List<Message>>($"/chat/{Connection.CurrentUser.IdUser}");

            Drivers = await Connection.Client.GetFromJsonAsync<List<Driver>>($"/drivers");

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
                $" {driver.IdDriverNavigation.Patronymic}",
                HorizontalAlignment = HorizontalAlignment.Left,
            };

            var nameTextBlock = new TextBlock()
            {
                Text = $"{driver.IdDriverNavigation.SecondName} {driver.IdDriverNavigation.FirstName}" +
                $" {driver.IdDriverNavigation.Patronymic}",
                HorizontalAlignment = HorizontalAlignment.Left,
                FontWeight = FontWeights.DemiBold,
            };
            var chatItemStackPanel = new StackPanel()
            {
                MaxHeight = 70,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(5),
                Width = 380,
            };
            chatItemStackPanel.Children.Add(nameTextBlock);

            if (message != null)
            {
                var today = DateTime.Now;

                var timeTextBlock = new TextBlock
                {
                    Text = $"{(message.Time.Date == today.Date
                    ? message.Time.ToString("HH:mm") :
                    message.Time.Year == today.Year
                    ? message.Time.ToString("dd MMM")
                    : message.Time.ToString("dd MMM yyyy"))}",
                    Foreground = new SolidColorBrush(Color.FromRgb(200, 197, 208)),
                    HorizontalAlignment = HorizontalAlignment.Right,
                };

                var messageTextTextBlock = new TextBlock
                {
                    Text = $"{(message.IdRecipient == driver.IdDriver ? "Вы: " : "")}{message.Value}",
                    Foreground = new SolidColorBrush(Color.FromRgb(229, 225, 230)),
                    HorizontalAlignment = HorizontalAlignment.Left,
                };

                chatItemStackPanel.Children.Add(timeTextBlock);
                chatItemStackPanel.Children.Add(messageTextTextBlock);
                //chatTextBlock.Text += $"\n{(message.Time.Date == today.Date
                //    ? message.Time.ToString("HH:mm") :
                //    message.Time.Year == today.Year
                //    ? message.Time.ToString("dd MMM")
                //    : message.Time.ToString("dd MMM yyyy"))}" +
                //    $"\n{(message.IdRecipient == driver.IdDriver ? "Вы: " : "")}{message.Value}";
            }

            var chatButton = new Button
            {
                Content = chatItemStackPanel,
                Tag = driver.IdDriver,
                //Width = 400,
                //HorizontalAlignment = HorizontalAlignment.Left,
                //HorizontalContentAlignment = HorizontalAlignment.Left,
                //Background = new SolidColorBrush(Color.FromRgb(70, 69, 89)),
                //Foreground = new SolidColorBrush(Color.FromRgb(227, 224, 249)),
            };
            chatButton.Style = Application.Current.Resources["chatItemButton"] as Style;
            chatButton.Click += (object sender, RoutedEventArgs e) => { Manager.MainFrame.Navigate(new CurrentChatPage(driver)); };
            return chatButton;
        }
    }
}
