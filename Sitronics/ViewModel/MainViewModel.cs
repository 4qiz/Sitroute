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
        private string userName = $"Здравствуйте, {Connection.CurrentUser.SecondName} {Connection.CurrentUser.FirstName} {Connection.CurrentUser?.Patronymic}";
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
        public ICommand ShowChatViewCommand { get; }

        public MainViewModel()
        {

            // Initialize commands
            ShowMapViewCommand = new ViewModelCommand(ExecuteShowMapViewCommand);
            ShowBusInfoViewCommand = new ViewModelCommand(ExecuteShowBusInfoViewCommand);
            ShowChatViewCommand = new ViewModelCommand(ExecuteShowChatViewCommand);

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

        private void ExecuteShowChatViewCommand(object obj)
        {
            CurrentChildView = new ChatViewModel();
            Caption = "Чат";
            Icon = IconChar.Message;
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