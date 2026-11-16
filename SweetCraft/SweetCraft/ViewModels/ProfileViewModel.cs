using SweetCraft.Data;
using SweetCraft.Helpers;
using SweetCraft.Services;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows.Input;

namespace SweetCraft.ViewModels
{
    public sealed class ProfileViewModel : ObservableObject
    {
        private readonly Users currentUser;
        private string firstName;
        private string lastName;
        private string middleName;
        private string email;
        private string phone;
        private string message;

        public ProfileViewModel(Users user)
        {
            currentUser = user ?? throw new ArgumentNullException(nameof(user));
            firstName = user.FirstName;
            lastName = user.LastName;
            middleName = user.MiddleName;
            email = user.Email;
            phone = user.Phone;
            SaveCommand = new RelayCommand(_ => Save());
            OrderHistory = new ObservableCollection<Orders>();
            using (var context = new SweetCraftDBEntities())
                foreach (var order in context.Orders.Include("OrderStatuses").Where(order => order.UserId == user.UserId).OrderByDescending(order => order.CreatedAt).ToList()) OrderHistory.Add(order);
        }

        public ObservableCollection<Orders> OrderHistory { get; }
        public ICommand SaveCommand { get; }
        public string RoleName => currentUser.Roles.RoleName;
        public string FirstName { get => firstName; set => SetProperty(ref firstName, value); }
        public string LastName { get => lastName; set => SetProperty(ref lastName, value); }
        public string MiddleName { get => middleName; set => SetProperty(ref middleName, value); }
        public string Email { get => email; set => SetProperty(ref email, value); }
        public string Phone { get => phone; set => SetProperty(ref phone, value); }
        public string Message { get => message; private set => SetProperty(ref message, value); }

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName) || FirstName.Trim().Length > 50 || LastName.Trim().Length > 50) { Message = "Укажите корректные имя и фамилию (до 50 символов)."; return; }
            if (MiddleName != null && MiddleName.Trim().Length > 50) { Message = "Отчество должно содержать не более 50 символов."; return; }
            if (!ValidationService.IsValidEmail(Email) || Email.Trim().Length > 100) { Message = "Введите корректный email (до 100 символов)."; return; }
            if (!ValidationService.IsValidPhone(Phone)) { Message = "Введите корректный номер телефона."; return; }
            try
            {
                using (var context = new SweetCraftDBEntities())
                {
                    string normalizedEmail = Email.Trim().ToLowerInvariant();
                    string normalizedPhone = ValidationService.NormalizePhone(Phone);
                    if (context.Users.Any(item => item.UserId != currentUser.UserId && item.Email.ToLower() == normalizedEmail)) { Message = "Этот email уже используется."; return; }
                    if (context.Users.ToList().Any(item => item.UserId != currentUser.UserId && ValidationService.NormalizePhone(item.Phone) == normalizedPhone)) { Message = "Этот телефон уже используется."; return; }
                    var user = context.Users.Find(currentUser.UserId);
                    if (user == null) throw new InvalidOperationException("Учетная запись не найдена.");
                    user.FirstName = FirstName.Trim(); user.LastName = LastName.Trim();
                    user.MiddleName = string.IsNullOrWhiteSpace(MiddleName) ? null : MiddleName.Trim();
                    user.Email = normalizedEmail; user.Phone = normalizedPhone;
                    context.SaveChanges();
                    currentUser.FirstName = user.FirstName; currentUser.LastName = user.LastName; currentUser.MiddleName = user.MiddleName; currentUser.Email = user.Email; currentUser.Phone = user.Phone;
                }
                Message = "Данные профиля сохранены.";
            }
            catch (Exception exception) { Message = ValidationService.GetUserFriendlyError(exception, "Не удалось сохранить данные профиля."); }
        }
    }
}
