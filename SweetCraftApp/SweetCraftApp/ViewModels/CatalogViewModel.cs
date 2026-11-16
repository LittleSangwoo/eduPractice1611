using SweetCraftApp.Data;
using SweetCraftApp.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetCraftApp.ViewModels
{
    public class CatalogViewModel : ObservableObject
    {
        private readonly SweetCraftDBEntities _context;
        public ObservableCollection<ProductViewModel> Products { get; set; }

        // Логика корзины
        public RelayCommand AddToCartCommand { get; }

        public CatalogViewModel()
        {
            _context = new SweetCraftDBEntities();
            LoadProducts();
            AddToCartCommand = new RelayCommand(AddToCart);
        }

        private void LoadProducts()
        {
            // Загружаем только доступные товары
            var productsDb = _context.Products.Where(p => p.IsAvailable).ToList(); 
            Products = new ObservableCollection<ProductViewModel>(productsDb.Select(p => new ProductViewModel(p)));
            OnPropertyChanged(nameof(Products));
        }

        private void AddToCart(object parameter)
        {
            if (Services.AuthService.CurrentUser == null || Services.AuthService.CurrentUser.RoleId == 1) // Гость[cite: 2]
            {
                System.Windows.MessageBox.Show("Для оформления предзаказа необходимо авторизоваться.");
                return;
            }
            // Логика добавления в CartItems
        }
    }

    // ViewModel-обертка для отображения бизнес-логики UI (Premium, Скидки)
    public class ProductViewModel
    {
        public Products Product { get; }
        public bool IsPremium => Product.Price > 1000; // Требование F3
        public bool HasDiscount => Product.OldPrice != null && Product.DiscountPercent != null; // Требование F3
        public decimal CurrentPrice => Product.Price;
        public decimal? OldPrice => Product.OldPrice;

        public ProductViewModel(Products product) => Product = product;
    }
}
