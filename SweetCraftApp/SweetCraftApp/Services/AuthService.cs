using SweetCraftApp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SweetCraftApp.Services
{
    public class AuthService
    {
        private readonly SweetCraftDBEntities _context;
        public static Users CurrentUser { get; private set; } // Глобальный доступ к текущему пользователю

        public AuthService(SweetCraftDBEntities context) => _context = context;

        public bool Login(string login, string password)
        {
            string hash = GenerateSHA256(password); //[cite: 1]
            var user = _context.Users.FirstOrDefault(u =>
                (u.Email == login || u.Phone == login) && u.PasswordHash == hash && u.IsActive); //[cite: 1]

            if (user != null)
            {
                user.LastLoginAt = DateTime.Now;
                _context.SaveChanges();
                CurrentUser = user;
                return true;
            }
            return false;
        }

        private string GenerateSHA256(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(bytes).Replace("-", "").ToUpper(); // Совместимость с HASHBYTES SQL[cite: 1]
            }
        }
    }
}
