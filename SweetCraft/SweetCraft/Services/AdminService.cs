using SweetCraft.Data;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace SweetCraft.Services
{
    public sealed class AdminService : IDisposable
    {
        private readonly SweetCraftDBEntities context;
        private readonly Dictionary<string, Type> tableTypes;
        private readonly bool isAdministrator;

        public AdminService(Users user)
        {
            if (user == null || user.Roles == null || (user.Roles.RoleName != "Менеджер" && user.Roles.RoleName != "Администратор"))
                throw new UnauthorizedAccessException("Недостаточно прав для управления таблицами.");
            isAdministrator = user.Roles.RoleName == "Администратор";
            context = new SweetCraftDBEntities();
            tableTypes = new Dictionary<string, Type>
            {
                { "Корзина (CartItems)", typeof(CartItems) }, { "Категории (Categories)", typeof(Categories) },
                { "Украшения (Decorations)", typeof(Decorations) }, { "Способы доставки (DeliveryMethods)", typeof(DeliveryMethods) },
                { "Начинки (Fillings)", typeof(Fillings) }, { "Позиции заказов (OrderItems)", typeof(OrderItems) },
                { "Заказы (Orders)", typeof(Orders) }, { "Статусы заказов (OrderStatuses)", typeof(OrderStatuses) },
                { "История статусов (OrderStatusHistory)", typeof(OrderStatusHistory) }, { "Способы оплаты (PaymentMethods)", typeof(PaymentMethods) },
                { "Товары (Products)", typeof(Products) }, { "Отзывы (Reviews)", typeof(Reviews) }, { "Роли (Roles)", typeof(Roles) }
            };
            if (isAdministrator) tableTypes.Add("Пользователи (Users)", typeof(Users));
        }

        public IEnumerable<string> TableNames => tableTypes.Keys;

        public IList<object> Load(string tableName)
        {
            Type type = GetType(tableName);
            return ((IEnumerable)context.Set(type)).Cast<object>().ToList();
        }

        public object Add(string tableName)
        {
            Type type = GetType(tableName);
            object entity = Activator.CreateInstance(type);
            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!property.CanWrite || property.GetIndexParameters().Length != 0 || property.Name.EndsWith("Id", StringComparison.Ordinal) && property.PropertyType == typeof(int)) continue;
                Type valueType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                if (valueType == typeof(string)) property.SetValue(entity, string.Empty, null);
                else if (valueType == typeof(DateTime)) property.SetValue(entity, DateTime.Now, null);
                else if (valueType == typeof(bool)) property.SetValue(entity, false, null);
                else if (valueType == typeof(int)) property.SetValue(entity, 1, null);
                else if (valueType == typeof(decimal)) property.SetValue(entity, 0m, null);
                else if (valueType == typeof(byte)) property.SetValue(entity, (byte)0, null);
            }
            context.Set(type).Add(entity);
            return entity;
        }

        public void Delete(string tableName, object entity)
        {
            if (!isAdministrator) throw new UnauthorizedAccessException("Менеджеру запрещено удалять записи.");
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            Type type = GetType(tableName);
            if (type == typeof(Products))
            {
                var product = (Products)entity;
                if (context.OrderItems.Any(item => item.ProductId == product.ProductId))
                    throw new InvalidOperationException("Невозможно удалить товар, так как он присутствует в одном или нескольких заказах.");
            }
            context.Set(type).Remove(entity);
        }

        public void SaveChanges()
        {
            foreach (var entry in context.ChangeTracker.Entries().Where(entry => entry.State == EntityState.Added || entry.State == EntityState.Modified).ToList())
            {
                string validationError = ValidationService.ValidateEntity(entry.Entity);
                if (validationError != null) throw new InvalidOperationException(validationError);
                ValidateReferences(entry.Entity);
            }
            ValidateUniqueUsers();
            context.SaveChanges();
        }

        private void ValidateReferences(object entity)
        {
            var product = entity as Products;
            if (product != null && !context.Categories.Any(item => item.CategoryId == product.CategoryId))
                throw new InvalidOperationException("Выбранная категория товара не существует.");

            var user = entity as Users;
            if (user != null && !context.Roles.Any(item => item.RoleId == user.RoleId))
                throw new InvalidOperationException("Выбранная роль пользователя не существует.");

            var cartItem = entity as CartItems;
            if (cartItem != null)
            {
                if (!context.Users.Any(item => item.UserId == cartItem.UserId) || !context.Products.Any(item => item.ProductId == cartItem.ProductId))
                    throw new InvalidOperationException("Выбранный пользователь или товар для корзины не существует.");
                if (cartItem.FillingId.HasValue && !context.Fillings.Any(item => item.FillingId == cartItem.FillingId.Value))
                    throw new InvalidOperationException("Выбранная начинка не существует.");
                if (cartItem.DecorationId.HasValue && !context.Decorations.Any(item => item.DecorationId == cartItem.DecorationId.Value))
                    throw new InvalidOperationException("Выбранное украшение не существует.");
            }

            var order = entity as Orders;
            if (order != null && (!context.Users.Any(item => item.UserId == order.UserId)
                || !context.OrderStatuses.Any(item => item.StatusId == order.StatusId)
                || !context.DeliveryMethods.Any(item => item.DeliveryMethodId == order.DeliveryMethodId)
                || !context.PaymentMethods.Any(item => item.PaymentMethodId == order.PaymentMethodId)))
                throw new InvalidOperationException("Один из связанных пользователя, статуса, способа доставки или оплаты не существует.");

            if (order != null && order.ManagerId.HasValue && !context.Users.Any(item => item.UserId == order.ManagerId.Value))
                throw new InvalidOperationException("Указанный менеджер заказа не существует.");

            var orderItem = entity as OrderItems;
            if (orderItem != null && (!context.Orders.Any(item => item.OrderId == orderItem.OrderId) || !context.Products.Any(item => item.ProductId == orderItem.ProductId)))
                throw new InvalidOperationException("Выбранный заказ или товар не существует.");
            if (orderItem != null && orderItem.FillingId.HasValue && !context.Fillings.Any(item => item.FillingId == orderItem.FillingId.Value))
                throw new InvalidOperationException("Выбранная начинка не существует.");
            if (orderItem != null && orderItem.DecorationId.HasValue && !context.Decorations.Any(item => item.DecorationId == orderItem.DecorationId.Value))
                throw new InvalidOperationException("Выбранное украшение не существует.");

            var history = entity as OrderStatusHistory;
            if (history != null && (!context.Orders.Any(item => item.OrderId == history.OrderId) || !context.OrderStatuses.Any(item => item.StatusId == history.StatusId)))
                throw new InvalidOperationException("Выбранный заказ или статус для истории не существует.");
            if (history != null && history.ChangedByUserId.HasValue && !context.Users.Any(item => item.UserId == history.ChangedByUserId.Value))
                throw new InvalidOperationException("Указанный пользователь в истории не существует.");

            var review = entity as Reviews;
            if (review != null && (!context.Users.Any(item => item.UserId == review.UserId) || !context.Products.Any(item => item.ProductId == review.ProductId)))
                throw new InvalidOperationException("Выбранный пользователь или товар для отзыва не существует.");
        }

        private void ValidateUniqueUsers()
        {
            var users = context.ChangeTracker.Entries<Users>()
                .Where(entry => entry.State == EntityState.Added || entry.State == EntityState.Modified)
                .Select(entry => entry.Entity)
                .ToList();
            int[] changedUserIds = users.Where(user => user.UserId > 0).Select(user => user.UserId).ToArray();

            for (int index = 0; index < users.Count; index++)
            {
                string email = users[index].Email.Trim().ToLowerInvariant();
                string phone = ValidationService.NormalizePhone(users[index].Phone);
                if (context.Users.Any(item => !changedUserIds.Contains(item.UserId) && item.Email.ToLower() == email))
                    throw new InvalidOperationException("Пользователь с таким email уже существует.");
                if (context.Users.Where(item => !changedUserIds.Contains(item.UserId)).ToList().Any(item => ValidationService.NormalizePhone(item.Phone) == phone))
                    throw new InvalidOperationException("Пользователь с таким телефоном уже существует.");

                for (int otherIndex = index + 1; otherIndex < users.Count; otherIndex++)
                {
                    if (string.Equals(email, users[otherIndex].Email.Trim(), StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("Пользователь с таким email уже существует.");
                    if (phone == ValidationService.NormalizePhone(users[otherIndex].Phone))
                        throw new InvalidOperationException("Пользователь с таким телефоном уже существует.");
                }
            }
        }

        private Type GetType(string tableName)
        {
            if (tableName == null || !tableTypes.TryGetValue(tableName, out Type type)) throw new InvalidOperationException("Выбранная таблица недоступна.");
            return type;
        }

        public void Dispose() => context.Dispose();

        public List<object> GetAllByType(Type type)
        {
            if (type == null || !tableTypes.Values.Contains(type)) throw new InvalidOperationException("Выбранная таблица недоступна.");
            return ((IEnumerable)context.Set(type)).Cast<object>().ToList();
        }
    }
}
