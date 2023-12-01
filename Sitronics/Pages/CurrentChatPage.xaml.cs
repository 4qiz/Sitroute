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
    /// Логика взаимодействия для CurrentChatPage.xaml
    /// </summary>
    public partial class CurrentChatPage : Page
    {
        Driver currentDriver;
        public CurrentChatPage(Driver driver)
        {
            InitializeComponent();

            currentDriver = driver;

            scrollViewer.ScrollToBottom();

            System.Windows.Threading.DispatcherTimer updateTimer = new System.Windows.Threading.DispatcherTimer();
            updateTimer.Tick += new EventHandler(UpdateTimer_Tick);
            updateTimer.Interval = new TimeSpan(0, 0, 30);
            updateTimer.Start();

            LoadData();
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            chatStackPanel.Children.Clear();
            List<Message> messages;
            using (var context = new SitrouteDataContext())
            {
                messages = context.Messages.AsNoTracking().ToList();
            }
            foreach (var message in messages)
            {
                if (message.IdSender == Connection.CurrentUser.IdUser && message.IdRecipient == currentDriver.IdDriver)
                    AddMessage(message, HorizontalAlignment.Right, Color.FromRgb(70, 69, 89)); // secondaryContainer
                else if (message.IdRecipient == Connection.CurrentUser.IdUser
                    && message.IdSender == currentDriver.IdDriver)
                    AddMessage(message, HorizontalAlignment.Left, Color.FromRgb(54, 52, 61)); //surface1
                else if (message.IdRecipient == null && message.IdSender == currentDriver.IdDriver)
                    AddMessage(message, HorizontalAlignment.Left, Color.FromRgb(54, 52, 61), false);
            }
        }

        private void AddMessage(Message message, HorizontalAlignment horizontalAlignment, Color bgColor, bool isReplied = true)
        {
            Color textColor;
            CornerRadius cornerRadius;
            if (horizontalAlignment == HorizontalAlignment.Left)
            {
                cornerRadius = new CornerRadius(25, 25, 25, 5);
                textColor = Color.FromRgb(229, 225, 230); //onSurface
            }
            else
            {
                cornerRadius = new CornerRadius(25, 25, 5, 25);
                textColor = Color.FromRgb(227, 224, 249); // onSecondaryContainer
            }

            var messageText = new StackPanel();
            messageText.Children.Add(
                new TextBlock()
                {
                    Text = $"{message.Value}",
                    FontSize = 18,
                    Foreground = new SolidColorBrush(textColor),
                    TextWrapping = TextWrapping.Wrap
                });
            messageText.Children.Add(new TextBlock()
            {
                Text = $"{message.Time}",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(200, 197, 208)),
                HorizontalAlignment = HorizontalAlignment.Left,
                TextWrapping = TextWrapping.NoWrap
            });

            chatStackPanel.Children.Add(
                new Border()
                {
                    BorderBrush = new SolidColorBrush(bgColor),
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(7),
                    CornerRadius = cornerRadius,
                    Background = new SolidColorBrush(bgColor),
                    HorizontalAlignment = horizontalAlignment,
                    Padding = new Thickness(11),
                    Child = messageText

                });
            if (!isReplied)
            {
                replyButton.Visibility = Visibility.Visible;
                newMessageTextBox.Visibility = Visibility.Collapsed;
                sendMessageButton.Visibility = Visibility.Collapsed;
            }
        }

        private void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            if (newMessageTextBox.Text != null && newMessageTextBox.Text != "")
            {
                using (var context = new SitrouteDataContext())
                {
                    context.Messages.Add(new Message()
                    {
                        Value = newMessageTextBox.Text,
                        IdSender = Connection.CurrentUser.IdUser,
                        IdRecipient = currentDriver.IdDriver
                    });
                    context.SaveChanges();
                    LoadData();
                    newMessageTextBox.Text = null;
                }
            }
        }

        private void ReplyButton_Click(object sender, RoutedEventArgs e)
        {
            replyButton.Visibility = Visibility.Collapsed;
            newMessageTextBox.Visibility = Visibility.Visible;
            sendMessageButton.Visibility = Visibility.Visible;
            using (var context = new SitrouteDataContext())
            {
                context.Messages.Where(m => m.IdRecipient == null && m.IdSender == currentDriver.IdDriver).ExecuteUpdate(setters => setters.SetProperty(m => m.IdRecipient, Connection.CurrentUser.IdUser));
            }
        }
    }
}
