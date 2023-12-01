using Microsoft.EntityFrameworkCore;
using Sitronics.Data;
using Sitronics.Models;
using Sitronics.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
                    AddMessage(message, HorizontalAlignment.Right, Color.FromRgb(140, 30, 255));
                else if (message.IdRecipient == Connection.CurrentUser.IdUser
                    && message.IdSender == currentDriver.IdDriver)
                    AddMessage(message, HorizontalAlignment.Left, Color.FromRgb(255, 255, 255));
                else if (message.IdRecipient == null && message.IdSender == currentDriver.IdDriver)
                    AddMessage(message, HorizontalAlignment.Left, Color.FromRgb(255, 255, 255), false);
            }
        }

        private void AddMessage(Message message, HorizontalAlignment horizontalAlignment, Color color, bool isReplied = true)
        {
            chatStackPanel.Children.Add(new Border()
            {
                BorderBrush = new SolidColorBrush(color),
                BorderThickness = new Thickness(5),
                Margin = new Thickness(5),
                HorizontalAlignment = horizontalAlignment,
                Child = new TextBlock()
                {
                    Text = $"{message.Value}\n{message.Time}",
                    FontSize = 20,
                    Foreground = new SolidColorBrush(color),
                    TextWrapping = TextWrapping.Wrap
                }
            });
            if(!isReplied)
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
            using(var context = new SitrouteDataContext())
            {
                context.Messages.Where(m=>m.IdRecipient == null && m.IdSender == currentDriver.IdDriver).ExecuteUpdate(setters => setters.SetProperty(m => m.IdRecipient, Connection.CurrentUser.IdUser));
            }
        }
    }
}
