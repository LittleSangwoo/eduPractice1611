using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SweetCraftApp.Services
{
    public static class ValidationService
    {
        public static bool IsValidEmail(string email) =>
            !string.IsNullOrWhiteSpace(email) && Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"); //[cite: 1]

        public static bool IsValidPhone(string phone) =>
            !string.IsNullOrWhiteSpace(phone) && Regex.IsMatch(phone, @"^\+[0-9]{11,15}$"); //[cite: 1]

        public static bool IsValidReadyDate(DateTime readyAt, DateTime createdAt, int leadTimeHours) =>
            readyAt >= createdAt.AddHours(leadTimeHours); //[cite: 2]
    }
}
