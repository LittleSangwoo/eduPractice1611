using SweetCraft.Data;
using System;
using System.Linq;

namespace SweetCraft.Services
{
    public sealed class AuthService
    {
        private const string DefaultRoleName = "Пользователь";

        public Users Authenticate(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrEmpty(password)) return null;
            string normalizedLogin = login.Trim();
            using (var context = new SweetCraftDBEntities())
            {
                Users user;
                if (ValidationService.IsValidEmail(normalizedLogin))
                {
                    string email = normalizedLogin.ToLowerInvariant();
                    user = context.Users.Include("Roles").FirstOrDefault(item => item.Email.ToLower() == email);
                }
                else
                {
                    string phone = ValidationService.NormalizePhone(normalizedLogin);
                    user = context.Users.Include("Roles").ToList().FirstOrDefault(item => ValidationService.NormalizePhone(item.Phone) == phone);
                }

                if (user == null || !user.IsActive || user.Password != password) return null;
                user.LastLoginAt = DateTime.Now;
                context.SaveChanges();
                return user;
            }
        }

        public Users Register(string firstName, string lastName, string middleName, string email, string phone, string password)
        {
            if (middleName != null && middleName.Trim().Length > 50) throw new ArgumentException("Отчество должно содержать не более 50 символов.");
            string error = ValidationService.ValidateRegistration(firstName, lastName, email, phone, password);
            if (error != null) throw new ArgumentException(error);

            string normalizedEmail = email.Trim().ToLowerInvariant();
            string normalizedPhone = ValidationService.NormalizePhone(phone);
            using (var context = new SweetCraftDBEntities())
            {
                if (context.Users.Any(item => item.Email.ToLower() == normalizedEmail)) throw new InvalidOperationException("Пользователь с таким email уже зарегистрирован.");
                if (context.Users.ToList().Any(item => ValidationService.NormalizePhone(item.Phone) == normalizedPhone)) throw new InvalidOperationException("Пользователь с таким номером телефона уже зарегистрирован.");
                Roles role = context.Roles.FirstOrDefault(item => item.RoleName == DefaultRoleName);
                if (role == null) throw new InvalidOperationException("В базе данных не найдена роль «Пользователь». Проверьте справочник ролей.");

                var user = new Users
                {
                    RoleId = role.RoleId,
                    FirstName = firstName.Trim(),
                    LastName = lastName.Trim(),
                    MiddleName = string.IsNullOrWhiteSpace(middleName) ? null : middleName.Trim(),
                    Email = normalizedEmail,
                    Phone = normalizedPhone,
                    Password = password,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                context.Users.Add(user);
                context.SaveChanges();
                user.Roles = role;
                return user;
            }
        }
    }
}
