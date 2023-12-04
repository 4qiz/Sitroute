using Sitronics.Models;
using Sitronics.Repositories;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Sitronics.ViewModel
{
    public class LoginViewModel : ViewModelBase
    {
        //Fields
        private string _username;
        private string _password;
        private string _errorMessage;
        private bool _isViewVisible = true;

        //private IUserRepository userRepository;

        //Properties
        public string Username
        {
            get
            {
                return _username;
            }
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        public string Password
        {
            get
            {
                return _password;
            }

            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }

            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public bool IsViewVisible
        {
            get
            {
                return _isViewVisible;
            }

            set
            {
                _isViewVisible = value;
                OnPropertyChanged(nameof(IsViewVisible));
            }
        }

        // Commands
        public ICommand LoginCommand { get; }
        public ICommand RecoverPasswordCommand { get; }
        public ICommand ShowPasswordCommand { get; }
        public ICommand RememberPasswordCommand { get; }

        //Constructor
        public LoginViewModel()
        {
            //userRepository = new UserRepository();
            LoginCommand = new ViewModelCommand(ExecuteLoginCommand, CanExecuteLoginCommand);
            //RecoverPasswordCommand = new ViewModelCommand(p => ExecuteRecoverPassCommand("", ""));
        }

        private bool CanExecuteLoginCommand(object obj)
        {
            bool validData;
            if (string.IsNullOrWhiteSpace(Username) || Username.Length <= 1 ||
                Password == null || Password.Length <= 1)
            {
                validData = false;
            }
            else
                validData = true;
            return validData;
        }

        private async void ExecuteLoginCommand(object obj)
        {
            try
            {

            var user = await Connection.Client.GetFromJsonAsync<User>($"/admins/{Username}/{Password}");

            if (user.IdUser == 0)
            {
                ErrorMessage = "* Неправильный логин или пароль";
            }
            else if (user.Admin != null)
            {
                Connection.CurrentUser = user;
                IsViewVisible = false;
            }
            else
            {
                ErrorMessage = "У вас нет прав для этого приложения";
            }

            }
            catch (Exception ex) 
            {
                MessageBox.Show(ex.Message);
            }
        }

        //static byte[] ComputeSha256Hash(string rawData)
        //{
        //    using (SHA256 sha256Hash = SHA256.Create())
        //        return sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        //}

        private void ExecuteRecoverPassCommand(string username, string email)
        {
            throw new NotImplementedException();
        }

    }
}
