using SweetCraft.Data;
using SweetCraft.Helpers;
using SweetCraft.Services;
using System;
using System.Windows.Input;

namespace SweetCraft.ViewModels
{
    public sealed class AuthViewModel : ObservableObject
    {
        private readonly AuthService authService = new AuthService();
        private string emailOrPhone;
        private string firstName;
        private string lastName;
        private string middleName;
        private string email;
        private string phone;
        private string password;
        private string registrationPassword;
        private string message;

        public AuthViewModel()
        {
            LoginCommand = new RelayCommand(_ => Login());
            RegisterCommand = new RelayCommand(_ => Register());
        }

        public event Action<Users> LoginSucceeded;
        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }
        public string EmailOrPhone { get => emailOrPhone; set => SetProperty(ref emailOrPhone, value); }
        public string FirstName { get => firstName; set => SetProperty(ref firstName, value); }
        public string LastName { get => lastName; set => SetProperty(ref lastName, value); }
        public string MiddleName { get => middleName; set => SetProperty(ref middleName, value); }
        public string Email { get => email; set => SetProperty(ref email, value); }
        public string Phone { get => phone; set => SetProperty(ref phone, value); }
        public string Password { get => password; set => SetProperty(ref password, value); }
        public string RegistrationPassword { get => registrationPassword; set => SetProperty(ref registrationPassword, value); }
        public string Message { get => message; private set => SetProperty(ref message, value); }

        private void Login()
        {
            try
            {
                Users user = authService.Authenticate(EmailOrPhone, Password);
                if (user == null) { Message = "Проверьте email/телефон и пароль. Учетная запись может быть отключена."; return; }
                LoginSucceeded?.Invoke(user);
            }
            catch (Exception exception) { Message = ValidationService.GetUserFriendlyError(exception, "Не удалось выполнить вход."); }
        }

        private void Register()
        {
            try
            {
                Users user = authService.Register(FirstName, LastName, MiddleName, Email, Phone, RegistrationPassword);
                Message = "Регистрация завершена. Вы вошли как «Пользователь».";
                LoginSucceeded?.Invoke(user);
            }
            catch (Exception exception) { Message = ValidationService.GetUserFriendlyError(exception, "Не удалось зарегистрировать пользователя."); }
        }
    }
}
