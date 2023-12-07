using FontAwesome.Sharp;
using Sitronics.Models;
using Sitronics.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Sitronics.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        //Fields
        private User _currentUserAccount;
        private string userName = $"{Connection.CurrentUser.SecondName} {Connection.CurrentUser.FirstName} {Connection.CurrentUser.Patronymic}";
        private ViewModelBase _currentChildView;
        private string _caption;
        private IconChar _icon;

        //private IUserRepository userRepository;

        //Properties
        public User CurrentUserAccount
        {
            get
            {
                return _currentUserAccount;
            }
            set
            {
                _currentUserAccount = value;
                OnPropertyChanged(nameof(CurrentUserAccount));
            }
        }

        public ViewModelBase CurrentChildView
        {
            get
            {
                return _currentChildView;
            }
            set
            {
                _currentChildView = value;
                OnPropertyChanged(nameof(CurrentChildView));
            }
        }
        public string Caption
        {
            get
            {
                return _caption;
            }
            set
            {
                _caption = value;
                OnPropertyChanged(nameof(Caption));
            }
        }
        public IconChar Icon
        {
            get
            {
                return _icon;
            }
            set
            {
                _icon = value;
                OnPropertyChanged(nameof(Icon));
            }
        }
        public string UserDisplayName
        {
            get { return userName; }
            set
            {
                userName = value;
                OnPropertyChanged(nameof(UserDisplayName));
            }
        }

        //--> Commands
        public ICommand ShowMapViewCommand { get; }
        public ICommand ShowBusInfoViewCommand { get; }
        public ICommand ShowRouteInfoViewCommand { get; }
        public ICommand ShowStopInfoViewCommand { get; }
        public ICommand ShowScheduleViewCommand { get; }
        public ICommand ShowDriverViewCommand { get; }
        public ICommand ShowChatViewCommand { get; }
        public ICommand ShowSettingsViewCommand { get; }

        public MainViewModel()
        {
            // Initialize commands
            ShowMapViewCommand = new ViewModelCommand(ExecuteShowMapViewCommand);
            ShowBusInfoViewCommand = new ViewModelCommand(ExecuteShowBusInfoViewCommand);
            ShowRouteInfoViewCommand = new ViewModelCommand(ExecuteShowRouteInfoViewCommand);
            ShowStopInfoViewCommand = new ViewModelCommand(ExecuteShowStopInfoViewCommand);
            ShowScheduleViewCommand = new ViewModelCommand(ExecuteShowScheduleViewCommand);
            ShowDriverViewCommand = new ViewModelCommand(ExecuteShowDriverViewCommand);
            ShowChatViewCommand = new ViewModelCommand(ExecuteShowChatViewCommand);
            ShowSettingsViewCommand = new ViewModelCommand(ExecuteShowSettingsViewCommand);

            //Default view
            ExecuteShowMapViewCommand(null);

            LoadCurrentUserData();
        }

        private void ExecuteShowMapViewCommand(object obj)
        {
            CurrentChildView = new MapViewModel();
            Caption = "Карта";
            Icon = IconChar.Map;
        }

        private void ExecuteShowBusInfoViewCommand(object obj)
        {
            CurrentChildView = new BusInfoViewModel();
            Caption = "Автобусы";
            Icon = IconChar.Bus;
        }

        private void ExecuteShowRouteInfoViewCommand(object obj)
        {
            CurrentChildView = new RouteInfoViewModel();
            Caption = "Маршруты";
            Icon = IconChar.LocationPin;
        }

        private void ExecuteShowStopInfoViewCommand(object obj)
        {
            CurrentChildView = new StopInfoViewModel();
            Caption = "Остановки";
            Icon = IconChar.Stop;
        }

        private void ExecuteShowScheduleViewCommand(object obj)
        {
            CurrentChildView = new ScheduleViewModel();
            Caption = "Расписание";
            Icon = IconChar.CalendarDays;
        }

        private void ExecuteShowDriverViewCommand(object obj)
        {
            CurrentChildView = new DriverViewModel();
            Caption = "Водитель";
            Icon = IconChar.DriversLicense;
        }

        private void ExecuteShowChatViewCommand(object obj)
        {
            CurrentChildView = new ChatViewModel();
            Caption = "Чат";
            Icon = IconChar.Message;
        }

        private void ExecuteShowSettingsViewCommand(object obj)
        {
            CurrentChildView = new SettingsViewModel();
            Caption = "Справка";
            Icon = IconChar.QuestionCircle;
        }

        private void LoadCurrentUserData()
        {
            //var user = userRepository.GetByUsername(Thread.CurrentPrincipal.Identity.Name);
            //if (user == null)
            //{
            //    CurrentUserAccount.DisplayName = "Invalid user, not logged in";
            //    return;
            //}
            //CurrentUserAccount.Username = user.Username;
            //CurrentUserAccount.DisplayName = $"Welcome {user.Name} {user.LastName}";
            //CurrentUserAccount.ProfilePicture = null;
        }
    }
}