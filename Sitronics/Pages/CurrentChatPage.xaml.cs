using Microsoft.EntityFrameworkCore;
using Sitronics.Data;
using Sitronics.Models;
using Sitronics.Repositories;
using System.Net.Http.Json;
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

            Manager.MainTimer.Tick += new EventHandler(UpdateTimer_Tick);
        }

        private async void UpdateTimer_Tick(object sender, EventArgs e)
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            chatStackPanel.Children.Clear();
            List<Message> messages;
            messages = await Connection.Client.GetFromJsonAsync<List<Message>>(
                $"/chat/{currentDriver.IdDriver}/{Connection.CurrentUser.IdUser}");
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
                newMessageBorder.Visibility = Visibility.Collapsed;
                sendMessageButton.Visibility = Visibility.Collapsed;
            }
        }

        private async void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            if (newMessageTextBox.Text != null && newMessageTextBox.Text != "")
            {
                var message = new Message()
                {
                    Value = newMessageTextBox.Text,
                    IdSender = Connection.CurrentUser.IdUser,
                    IdRecipient = currentDriver.IdDriver
                };
                await Connection.Client.PostAsJsonAsync("/message", message);
                await LoadData();
                newMessageTextBox.Text = null;
            }
        }

        private void ReplyButton_Click(object sender, RoutedEventArgs e)
        {
            replyButton.Visibility = Visibility.Collapsed;
            newMessageBorder.Visibility = Visibility.Visible;
            sendMessageButton.Visibility = Visibility.Visible;
            Connection.Client.PatchAsJsonAsync($"/message/reply", new Message 
            { IdSender = currentDriver.IdDriver, IdRecipient = Connection.CurrentUser.IdUser});
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadData();
        }
    }
}
