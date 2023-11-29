using Microsoft.EntityFrameworkCore;
using Sitronics.Data;
using Sitronics.Repositories;
using System.Net;
using System.Security;
using System.Security.Cryptography;
using System.Security.Principal;
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

        private void ExecuteLoginCommand(object obj)
        {
            using (var context = new SitrouteDataContext())
            {
                var user = context.Users.Include(u => u.Admin)
                    .Include(u => u.MessageIdRecipientNavigations)
                    .Include(u => u.MessageIdSenderNavigations)
                    .Where(u => u.Login == Username.Trim())
                .FirstOrDefault();

                var hashInput = ComputeSha256Hash(Password);

                if (user != null &&
                    Convert.ToHexString(user.Password) == Convert.ToHexString(hashInput))
                {
                    if (user.Admin != null)
                    {
                        Connection.CurrentUser = user;
                        IsViewVisible = false;
                    }
                    else
                    {
                        ErrorMessage = "У вас нет прав для этого приложения";
                    }
                }
                else
                    ErrorMessage = "* Неправильный логин или пароль";
            }
        }

        static byte[] ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
                return sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        }

        private void ExecuteRecoverPassCommand(string username, string email)
        {
            throw new NotImplementedException();
        }

    }
}
