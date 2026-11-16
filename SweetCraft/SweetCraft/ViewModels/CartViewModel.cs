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
    public sealed class CartLineViewModel : ObservableObject
    {
        private int quantity;
        public int CartItemId { get; set; }
        public int ProductId { get; set; }
        public int? FillingId { get; set; }
        public string ProductName { get; set; }
        public string FillingName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get => quantity; set { if (SetProperty(ref quantity, value)) OnPropertyChanged(nameof(LineTotal)); } }
        public decimal LineTotal => UnitPrice * Quantity;
    }

    public sealed class CartViewModel : ObservableObject
    {
        private readonly Users currentUser;
        private readonly OrderService orderService = new OrderService();
        private CartLineViewModel selectedLine;
        private DateTime readyAt = DateTime.Today.AddDays(7);
        private string phone;
        private string address;
        private string comment;
        private int? selectedDeliveryId;
        private int? selectedPaymentId;
        private string message;

        public CartViewModel(Users user)
        {
            currentUser = user ?? throw new ArgumentNullException(nameof(user));
            Lines = new ObservableCollection<CartLineViewModel>();
            DeliveryMethods = new ObservableCollection<DeliveryMethods>();
            PaymentMethods = new ObservableCollection<PaymentMethods>();
            phone = user.Phone;
            IncreaseCommand = new RelayCommand(_ => ChangeQuantity(1), _ => SelectedLine != null);
            DecreaseCommand = new RelayCommand(_ => ChangeQuantity(-1), _ => SelectedLine != null);
            RemoveCommand = new RelayCommand(_ => Remove(), _ => SelectedLine != null);
            PlaceOrderCommand = new RelayCommand(_ => PlaceOrder());
            Load();
        }

        public ObservableCollection<CartLineViewModel> Lines { get; }
        public ObservableCollection<DeliveryMethods> DeliveryMethods { get; }
        public ObservableCollection<PaymentMethods> PaymentMethods { get; }
        public ICommand IncreaseCommand { get; }
        public ICommand DecreaseCommand { get; }
        public ICommand RemoveCommand { get; }
        public ICommand PlaceOrderCommand { get; }
        public CartLineViewModel SelectedLine { get => selectedLine; set => SetProperty(ref selectedLine, value); }
        public DateTime ReadyAt { get => readyAt; set => SetProperty(ref readyAt, value); }
        public string Phone { get => phone; set => SetProperty(ref phone, value); }
        public string Address { get => address; set => SetProperty(ref address, value); }
        public string Comment { get => comment; set => SetProperty(ref comment, value); }
        public int? SelectedDeliveryId { get => selectedDeliveryId; set => SetProperty(ref selectedDeliveryId, value); }
        public int? SelectedPaymentId { get => selectedPaymentId; set => SetProperty(ref selectedPaymentId, value); }
        public string Message { get => message; private set => SetProperty(ref message, value); }
        public decimal Total => Lines.Sum(line => line.LineTotal);

        private void Load()
        {
            using (var context = new SweetCraftDBEntities())
            {
                foreach (var item in context.CartItems.Include("Products").Include("Fillings").Where(item => item.UserId == currentUser.UserId).ToList())
                {
                    Lines.Add(new CartLineViewModel { CartItemId = item.CartItemId, ProductId = item.ProductId, FillingId = item.FillingId, ProductName = item.Products.Name, FillingName = item.Fillings == null ? "Без начинки" : item.Fillings.Name, UnitPrice = PricingService.GetPriceWithOptions(item.Products, item.Fillings, null), Quantity = item.Quantity });
                }
                foreach (var item in context.DeliveryMethods.Where(method => method.IsActive).OrderBy(method => method.Name).ToList()) DeliveryMethods.Add(item);
                foreach (var item in context.PaymentMethods.Where(method => method.IsActive).OrderBy(method => method.Name).ToList()) PaymentMethods.Add(item);
            }
            OnPropertyChanged(nameof(Total));
        }

        private void ChangeQuantity(int change)
        {
            if (SelectedLine == null) return;
            if (change > 0 && SelectedLine.Quantity == int.MaxValue)
            {
                Message = "Достигнуто максимальное количество товара в корзине.";
                return;
            }
            int newQuantity = SelectedLine.Quantity + change;
            if (newQuantity < 1) return;
            try
            {
                using (var context = new SweetCraftDBEntities())
                {
                    var item = context.CartItems.Find(SelectedLine.CartItemId);
                    if (item == null) throw new InvalidOperationException("Позиция корзины больше не существует.");
                    item.Quantity = newQuantity;
                    context.SaveChanges();
                }
                SelectedLine.Quantity = newQuantity;
                OnPropertyChanged(nameof(Total));
            }
            catch (Exception exception) { Message = ValidationService.GetUserFriendlyError(exception, "Не удалось изменить количество товара."); }
        }

        private void Remove()
        {
            if (SelectedLine == null) return;
            try
            {
                using (var context = new SweetCraftDBEntities())
                {
                    var item = context.CartItems.Find(SelectedLine.CartItemId);
                    if (item != null) { context.CartItems.Remove(item); context.SaveChanges(); }
                }
                Lines.Remove(SelectedLine);
                SelectedLine = null;
                OnPropertyChanged(nameof(Total));
            }
            catch (Exception exception) { Message = ValidationService.GetUserFriendlyError(exception, "Не удалось удалить товар из корзины."); }
        }

        private void PlaceOrder()
        {
            try
            {
                if (!SelectedDeliveryId.HasValue || !SelectedPaymentId.HasValue) { Message = "Выберите способ доставки и оплаты."; return; }
                if (ReadyAt == default(DateTime) || ReadyAt <= DateTime.Now) { Message = "Выберите будущую дату готовности заказа."; return; }
                orderService.PlaceOrder(currentUser.UserId, ReadyAt, Phone, Address, SelectedDeliveryId.Value, SelectedPaymentId.Value, Comment);
                Lines.Clear();
                OnPropertyChanged(nameof(Total));
                Message = "Предзаказ успешно оформлен.";
            }
            catch (Exception exception) { Message = ValidationService.GetUserFriendlyError(exception, "Не удалось оформить заказ."); }
        }
    }
}
