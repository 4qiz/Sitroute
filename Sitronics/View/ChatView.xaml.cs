using Sitronics.Data;
using Sitronics.Models;
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

            scrollViewer.ScrollToBottom();

            List<Message> messages;
            using (var context = new SitrouteDataContext())
            {
                messages = context.Messages.ToList();
            }
            foreach (var message in messages)
            {
                if (message.IdSender == Connection.CurrentUser.IdUser)
                    AddMessage(message.Value, HorizontalAlignment.Right, Color.FromRgb(140, 30, 255));
                else if (message.IdRecipient == Connection.CurrentUser.IdUser
                    && message.IdSender == currentDriver.IdDriver)
                    AddMessage(message.Value, HorizontalAlignment.Left, Color.FromRgb(255, 255, 255));
            }
        }

        private void AddMessage(string message, HorizontalAlignment horizontalAlignment, Color color)
        {
            chatStackPanel.Children.Add(new Border()
            {
                BorderBrush = new SolidColorBrush(color),
                BorderThickness = new Thickness(5),
                Margin = new Thickness(5), 
                HorizontalAlignment = horizontalAlignment,
                Child = new TextBlock()
                {
                    Text = message,
                    FontSize = 20,
                    Foreground = new SolidColorBrush(color),
                    TextWrapping = TextWrapping.Wrap
                }
            });
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
                    AddMessage(newMessageTextBox.Text, HorizontalAlignment.Right, Color.FromRgb(140, 30, 255));
                    newMessageTextBox.Text = null;
                }
            }
        }
    }
}
