using Sitronics.Repositories;
using Sitronics.View;
using System.Windows;

namespace Sitronics
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
       
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            
            var loginView = new LoginWindow();
            loginView.Show();
            loginView.IsVisibleChanged += (s, ev) =>
            {
                if (loginView.IsVisible == false && loginView.IsLoaded)
                {
                    Manager.MainTimer.Interval = new TimeSpan(0, 0, 30);
                    Manager.MainTimer.Start();
                    var mainView = new MainWindow();
                    mainView.Show();
                }
            };
        }
    }
}

