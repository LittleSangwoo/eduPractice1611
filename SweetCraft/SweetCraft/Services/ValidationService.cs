using SweetCraft.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SweetCraft.Services
{
    public static class ValidationService
    {
        private static readonly Regex EmailPattern = new Regex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex PhonePattern = new Regex(@"^\+?[0-9\s()\-]{10,20}$", RegexOptions.Compiled);

        public static string ValidateRegistration(string firstName, string lastName, string email, string phone, string password)
        {
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName)) return "Укажите имя и фамилию.";
            if (firstName.Trim().Length > 50 || lastName.Trim().Length > 50) return "Имя и фамилия должны содержать не более 50 символов.";
            if (!string.IsNullOrWhiteSpace(email) && email.Trim().Length > 100) return "Адрес электронной почты слишком длинный.";
            if (string.IsNullOrWhiteSpace(email) || !EmailPattern.IsMatch(email.Trim())) return "Введите корректный адрес электронной почты.";
            if (!IsValidPhone(phone) || NormalizePhone(phone).Length > 16) return "Введите корректный номер телефона (от 10 до 15 цифр).";
            if (!IsValidPassword(password) || password.Length > 128) return "Пароль должен содержать от 8 до 128 символов, букву и цифру.";
            return null;
        }

        public static string ValidateEntity(object entity)
        {
            var errors = new List<string>();
            if (entity == null) return "Запись не выбрана.";

            var product = entity as Products;
            if (product != null)
            {
                Required(errors, "Название товара", product.Name, 120);
                MaxLength(errors, "Описание товара", product.Description, 1000);
                Money(errors, "Цена товара", product.Price, 99999999.99m, false);
                if (product.OldPrice.HasValue) Money(errors, "Старая цена", product.OldPrice.Value, 99999999.99m, false);
                if (product.DiscountPercent.HasValue && product.DiscountPercent.Value > 100) errors.Add("Скидка должна быть от 0 до 100 процентов.");
                if (product.OldPrice.HasValue != product.DiscountPercent.HasValue) errors.Add("Для скидки укажите одновременно старую цену и процент скидки.");
                if (product.ImagePath == null) errors.Add("Путь к изображению не может быть пустым.");
                MaxLength(errors, "Путь к изображению", product.ImagePath, 260);
                if (!string.IsNullOrWhiteSpace(product.ImagePath) && (Path.IsPathRooted(product.ImagePath) || !product.ImagePath.Replace('/', '\\').StartsWith("ProductImages\\", StringComparison.OrdinalIgnoreCase)))
                    errors.Add("Изображение должно иметь относительный путь внутри папки ProductImages.");
                if (product.CategoryId <= 0) errors.Add("Выберите категорию товара.");
                if (product.WeightGrams.HasValue && product.WeightGrams.Value <= 0) errors.Add("Вес товара должен быть больше нуля.");
                if (product.ShelfLifeHours < 0 || product.LeadTimeHours < 0) errors.Add("Срок хранения и время приготовления не могут быть отрицательными.");
            }
            else if (entity is Users user)
            {
                Required(errors, "Фамилия", user.LastName, 50);
                Required(errors, "Имя", user.FirstName, 50);
                MaxLength(errors, "Отчество", user.MiddleName, 50);
                if (!IsValidEmail(user.Email) || user.Email.Trim().Length > 100) errors.Add("Введите корректный email (не более 100 символов).");
                if (!IsValidPhone(user.Phone) || NormalizePhone(user.Phone).Length > 16) errors.Add("Введите корректный телефон (от 10 до 15 цифр).");
                if (!IsValidPassword(user.Password) || user.Password.Length > 128) errors.Add("Пароль должен содержать от 8 до 128 символов, букву и цифру.");
                if (user.RoleId <= 0) errors.Add("Выберите роль пользователя.");
            }
            else if (entity is Categories category)
            {
                Required(errors, "Название категории", category.Name, 60);
                MaxLength(errors, "Описание категории", category.Description, 300);
                NonNegative(errors, "Порядок сортировки", category.SortOrder);
            }
            else if (entity is Fillings filling)
            {
                Required(errors, "Название начинки", filling.Name, 60);
                MaxLength(errors, "Описание начинки", filling.Description, 300);
                Money(errors, "Доплата за начинку", filling.ExtraPrice, 99999999.99m, true);
            }
            else if (entity is Decorations decoration)
            {
                Required(errors, "Название украшения", decoration.Name, 60);
                MaxLength(errors, "Описание украшения", decoration.Description, 300);
                Money(errors, "Доплата за украшение", decoration.ExtraPrice, 99999999.99m, true);
            }
            else if (entity is DeliveryMethods delivery)
            {
                Required(errors, "Название способа доставки", delivery.Name, 60);
                Money(errors, "Стоимость доставки", delivery.Price, 99999999.99m, true);
            }
            else if (entity is PaymentMethods payment)
            {
                Required(errors, "Название способа оплаты", payment.Name, 60);
            }
            else if (entity is OrderStatuses status)
            {
                Required(errors, "Название статуса", status.Name, 40);
                NonNegative(errors, "Порядок статуса", status.SortOrder);
            }
            else if (entity is Roles role)
            {
                Required(errors, "Название роли", role.RoleName, 30);
                MaxLength(errors, "Описание роли", role.Description, 200);
                if (!string.IsNullOrWhiteSpace(role.RoleName) && role.RoleName != "Гость" && role.RoleName != "Пользователь" && role.RoleName != "Менеджер" && role.RoleName != "Администратор")
                    errors.Add("Допустимые роли: Гость, Пользователь, Менеджер и Администратор.");
            }
            else if (entity is CartItems cartItem)
            {
                if (cartItem.UserId <= 0 || cartItem.ProductId <= 0) errors.Add("Для позиции корзины укажите пользователя и товар.");
                if (cartItem.Quantity <= 0) errors.Add("Количество товара должно быть больше нуля.");
                MaxLength(errors, "Текст на товаре", cartItem.CustomText, 200);
            }
            else if (entity is OrderItems orderItem)
            {
                if (orderItem.OrderId <= 0 || orderItem.ProductId <= 0) errors.Add("Для позиции заказа укажите заказ и товар.");
                if (orderItem.Quantity <= 0) errors.Add("Количество товара должно быть больше нуля.");
                Money(errors, "Цена позиции заказа", orderItem.UnitPrice, 99999999.99m, true);
                MaxLength(errors, "Текст на товаре", orderItem.CustomText, 200);
            }
            else if (entity is Orders order)
            {
                Required(errors, "Номер заказа", order.OrderNumber, 20);
                if (order.UserId <= 0 || order.StatusId <= 0 || order.DeliveryMethodId <= 0 || order.PaymentMethodId <= 0) errors.Add("Укажите пользователя, статус, способ доставки и способ оплаты заказа.");
                if (!IsValidPhone(order.ContactPhone) || NormalizePhone(order.ContactPhone).Length > 16) errors.Add("Введите корректный телефон для связи.");
                MaxLength(errors, "Адрес доставки", order.DeliveryAddress, 300);
                MaxLength(errors, "Комментарий к заказу", order.Comment, 500);
                MaxLength(errors, "Причина отмены", order.CancelReason, 300);
                Money(errors, "Стоимость доставки", order.DeliveryCost, 99999999.99m, true);
                Money(errors, "Сумма заказа", order.TotalAmount, 9999999999.99m, true);
                if (order.ReadyAt < order.CreatedAt) errors.Add("Дата готовности заказа не может быть раньше даты создания.");
            }
            else if (entity is OrderStatusHistory history)
            {
                if (history.OrderId <= 0 || history.StatusId <= 0) errors.Add("Укажите заказ и статус для записи истории.");
                MaxLength(errors, "Примечание к статусу", history.Note, 300);
            }
            else if (entity is Reviews review)
            {
                if (review.UserId <= 0 || review.ProductId <= 0) errors.Add("Для отзыва укажите пользователя и товар.");
                if (review.Rating < 1 || review.Rating > 5) errors.Add("Оценка должна быть от 1 до 5.");
                MaxLength(errors, "Текст отзыва", review.Comment, 1000);
            }
            else
            {
                return "Этот тип записи нельзя проверить и сохранить через панель управления.";
            }

            return errors.Count == 0 ? null : string.Join(" ", errors);
        }

        public static bool IsValidEmail(string email) => !string.IsNullOrWhiteSpace(email) && EmailPattern.IsMatch(email.Trim());

        public static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone) || !PhonePattern.IsMatch(phone.Trim())) return false;
            int digits = 0;
            foreach (char character in phone) if (char.IsDigit(character)) digits++;
            return digits >= 10 && digits <= 15;
        }

        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8) return false;
            bool hasLetter = false;
            bool hasDigit = false;
            foreach (char character in password)
            {
                hasLetter |= char.IsLetter(character);
                hasDigit |= char.IsDigit(character);
            }
            return hasLetter && hasDigit;
        }

        public static string NormalizePhone(string phone)
        {
            if (phone == null) return string.Empty;
            string digits = Regex.Replace(phone, @"\D", string.Empty);
            return phone.TrimStart().StartsWith("+", StringComparison.Ordinal) ? "+" + digits : digits;
        }

        public static bool IsValidPreorderDate(DateTime readyAt, int minimumLeadTimeHours)
        {
            if (minimumLeadTimeHours < 0) return false;
            try { return readyAt >= DateTime.Now.AddHours(minimumLeadTimeHours); }
            catch (ArgumentOutOfRangeException) { return false; }
        }

        public static string GetUserFriendlyError(Exception exception, string operation)
        {
            if (exception is ArgumentException || exception is InvalidOperationException || exception is UnauthorizedAccessException)
                return exception.Message;
            return operation + " Не удалось выполнить операцию. Проверьте введённые данные и подключение к базе данных.";
        }

        private static void Required(ICollection<string> errors, string field, string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value)) errors.Add("Поле «" + field + "» обязательно.");
            MaxLength(errors, field, value, maxLength);
        }

        private static void MaxLength(ICollection<string> errors, string field, string value, int maxLength)
        {
            if (value != null && value.Length > maxLength) errors.Add("Поле «" + field + "» не должно превышать " + maxLength + " символов.");
        }

        private static void Money(ICollection<string> errors, string field, decimal value, decimal maximum, bool allowZero)
        {
            if (value < 0 || (!allowZero && value == 0)) errors.Add("Поле «" + field + "» должно быть " + (allowZero ? "неотрицательным" : "больше нуля") + ".");
            if (value > maximum) errors.Add("Поле «" + field + "» превышает допустимое значение.");
            if (decimal.Round(value, 2) != value) errors.Add("Поле «" + field + "» должно содержать не более двух знаков после запятой.");
        }

        private static void NonNegative(ICollection<string> errors, string field, decimal value)
        {
            if (value < 0) errors.Add("Поле «" + field + "» не может быть отрицательным.");
        }

        private static void NonNegative(ICollection<string> errors, string field, int value)
        {
            if (value < 0) errors.Add("Поле «" + field + "» не может быть отрицательным.");
        }
    }
}
