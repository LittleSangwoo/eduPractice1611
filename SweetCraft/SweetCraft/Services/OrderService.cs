using SweetCraft.Data;
using System;
using System.Linq;

namespace SweetCraft.Services
{
    public sealed class OrderService
    {
        public void PlaceOrder(int userId, DateTime readyAt, string phone, string address, int deliveryMethodId, int paymentMethodId, string comment)
        {
            if (userId <= 0) throw new ArgumentException("Не удалось определить пользователя для заказа.");
            if (!ValidationService.IsValidPhone(phone) || ValidationService.NormalizePhone(phone).Length > 16) throw new ArgumentException("Укажите корректный контактный телефон (от 10 до 15 цифр).");
            if (address != null && address.Trim().Length > 300) throw new ArgumentException("Адрес доставки не должен превышать 300 символов.");
            if (comment != null && comment.Trim().Length > 500) throw new ArgumentException("Комментарий не должен превышать 500 символов.");
            if (deliveryMethodId <= 0 || paymentMethodId <= 0) throw new ArgumentException("Выберите способ доставки и оплаты.");
            if (readyAt == default(DateTime) || readyAt <= DateTime.Now) throw new ArgumentException("Выберите будущую дату готовности заказа.");
            using (var context = new SweetCraftDBEntities())
            using (var transaction = context.Database.BeginTransaction())
            {
                var cart = context.CartItems.Include("Products").Include("Fillings").Include("Decorations").Where(item => item.UserId == userId).ToList();
                if (cart.Count == 0) throw new InvalidOperationException("Корзина пуста.");
                if (readyAt.Date == DateTime.MaxValue.Date) throw new ArgumentException("Выберите допустимую дату готовности заказа.");
                int minimumLeadTime = cart.Max(item => item.Products.LeadTimeHours);
                DateTime readyAtEndOfDay = readyAt.Date.AddDays(1).AddTicks(-1);
                if (!ValidationService.IsValidPreorderDate(readyAtEndOfDay, minimumLeadTime))
                    throw new ArgumentException("Дата готовности заказа должна учитывать минимальное время приготовления: " + minimumLeadTime + " ч.");

                var delivery = context.DeliveryMethods.FirstOrDefault(item => item.DeliveryMethodId == deliveryMethodId && item.IsActive);
                var payment = context.PaymentMethods.FirstOrDefault(item => item.PaymentMethodId == paymentMethodId && item.IsActive);
                var status = context.OrderStatuses.OrderBy(item => item.SortOrder).FirstOrDefault();
                if (delivery == null || payment == null || status == null) throw new InvalidOperationException("Проверьте справочники способов доставки, оплаты и статусов заказа.");

                if (cart.Any(item => item.Products == null || !item.Products.IsAvailable || item.Quantity <= 0))
                    throw new InvalidOperationException("В корзине есть недоступный товар или некорректное количество. Обновите корзину.");
                decimal itemsTotal = cart.Sum(item => PricingService.GetPriceWithOptions(item.Products, item.Fillings, item.Decorations) * item.Quantity);
                if (itemsTotal < 0 || itemsTotal + delivery.Price > 9999999999.99m)
                    throw new InvalidOperationException("Сумма заказа выходит за допустимые пределы.");
                var order = new Orders
                {
                    OrderNumber = "SC" + DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    UserId = userId,
                    StatusId = status.StatusId,
                    DeliveryMethodId = delivery.DeliveryMethodId,
                    PaymentMethodId = payment.PaymentMethodId,
                    CreatedAt = DateTime.Now,
                    ReadyAt = readyAtEndOfDay,
                    ContactPhone = ValidationService.NormalizePhone(phone),
                    DeliveryAddress = string.IsNullOrWhiteSpace(address) ? null : address.Trim(),
                    DeliveryCost = delivery.Price,
                    TotalAmount = itemsTotal + delivery.Price,
                    Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim()
                };
                context.Orders.Add(order);
                context.SaveChanges();

                foreach (var item in cart)
                {
                    decimal unitPrice = PricingService.GetPriceWithOptions(item.Products, item.Fillings, item.Decorations);
                    context.OrderItems.Add(new OrderItems
                    {
                        OrderId = order.OrderId,
                        ProductId = item.ProductId,
                        FillingId = item.FillingId,
                        DecorationId = item.DecorationId,
                        Quantity = item.Quantity,
                        UnitPrice = unitPrice,
                        CustomText = item.CustomText
                    });
                    context.CartItems.Remove(item);
                }
                context.OrderStatusHistory.Add(new OrderStatusHistory { OrderId = order.OrderId, StatusId = status.StatusId, ChangedAt = DateTime.Now, Note = "Заказ создан" });
                context.SaveChanges();
                transaction.Commit();
            }
        }
    }
}
