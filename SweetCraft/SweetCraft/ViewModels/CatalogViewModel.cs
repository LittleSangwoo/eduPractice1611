using SweetCraft.Data;
using SweetCraft.Helpers;
using SweetCraft.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace SweetCraft.ViewModels
{
    public sealed class CatalogViewModel : ObservableObject
    {
        private readonly Users currentUser;
        private readonly CatalogService catalogService = new CatalogService();
        private string searchText;
        private int? selectedCategoryId;
        private int? selectedFillingId;
        private string selectedSort = "По названию";
        private string statusMessage;

        public CatalogViewModel(Users user)
        {
            currentUser = user;
            Products = new ObservableCollection<ProductCardViewModel>();
            Categories = new ObservableCollection<Categories>(catalogService.GetCategories());
            Fillings = new ObservableCollection<Fillings>();
            using (var context = new SweetCraftDBEntities())
                foreach (var filling in context.Fillings.Where(item => item.IsActive).OrderBy(item => item.Name).ToList()) Fillings.Add(filling);
            RefreshCommand = new RelayCommand(_ => Refresh());
            ResetFiltersCommand = new RelayCommand(_ => ResetFilters());
            AddToCartCommand = new RelayCommand(product => AddToCart(product as ProductCardViewModel), _ => currentUser != null);
            Refresh();
        }

        public ObservableCollection<ProductCardViewModel> Products { get; }
        public ObservableCollection<Categories> Categories { get; }
        public ObservableCollection<Fillings> Fillings { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ResetFiltersCommand { get; }
        public ICommand AddToCartCommand { get; }
        public Users CurrentUser => currentUser;
        public bool IsSignedIn => currentUser != null;
        public bool CanManage => currentUser != null && (currentUser.Roles.RoleName == "Менеджер" || currentUser.Roles.RoleName == "Администратор");
        public bool IsAdministrator => currentUser != null && currentUser.Roles.RoleName == "Администратор";
        public string SearchText { get => searchText; set { if (SetProperty(ref searchText, value)) Refresh(); } }
        public int? SelectedCategoryId { get => selectedCategoryId; set { if (SetProperty(ref selectedCategoryId, value)) Refresh(); } }
        public int? SelectedFillingId { get => selectedFillingId; set { if (SetProperty(ref selectedFillingId, value)) Refresh(); } }
        public string SelectedSort { get => selectedSort; set { if (SetProperty(ref selectedSort, value)) Refresh(); } }
        public string StatusMessage { get => statusMessage; private set => SetProperty(ref statusMessage, value); }

        private void ResetFilters()
        {
            SearchText = null;
            SelectedCategoryId = null;
            SelectedFillingId = null;
            SelectedSort = "По названию";
        }

        private void Refresh()
        {
            if (Products == null) return;
            var products = catalogService.GetProducts().AsEnumerable();
            if (!string.IsNullOrWhiteSpace(SearchText)) products = products.Where(product => product.Name.IndexOf(SearchText.Trim(), StringComparison.CurrentCultureIgnoreCase) >= 0 || (product.Description ?? string.Empty).IndexOf(SearchText.Trim(), StringComparison.CurrentCultureIgnoreCase) >= 0);
            if (SelectedCategoryId.HasValue) products = products.Where(product => product.CategoryId == SelectedCategoryId.Value);
            if (SelectedFillingId.HasValue) products = products.Where(product => product.Fillings.Any(filling => filling.FillingId == SelectedFillingId.Value));
            switch (SelectedSort)
            {
                case "Цена по возрастанию": products = products.OrderBy(product => PricingService.GetProductPrice(product)); break;
                case "Цена по убыванию": products = products.OrderByDescending(product => PricingService.GetProductPrice(product)); break;
                default: products = products.OrderBy(product => product.Name); break;
            }
            Products.Clear();
            foreach (Products product in products) Products.Add(new ProductCardViewModel(product));
        }

        private void AddToCart(ProductCardViewModel card)
        {
            if (card == null || currentUser == null) return;
            try
            {
                using (var context = new SweetCraftDBEntities())
                {
                    var product = context.Products.FirstOrDefault(item => item.ProductId == card.Product.ProductId && item.IsAvailable);
                    if (product == null) throw new InvalidOperationException("Этот товар больше недоступен.");
                    var existing = context.CartItems.FirstOrDefault(item => item.UserId == currentUser.UserId && item.ProductId == product.ProductId && item.FillingId == SelectedFillingId);
                    if (existing == null)
                    {
                        context.CartItems.Add(new CartItems { UserId = currentUser.UserId, ProductId = product.ProductId, FillingId = SelectedFillingId, Quantity = 1, AddedAt = DateTime.Now });
                    }
                    else if (existing.Quantity == int.MaxValue) throw new InvalidOperationException("Достигнуто максимальное количество товара в корзине.");
                    else existing.Quantity++;
                    context.SaveChanges();
                }
                StatusMessage = "Товар добавлен в корзину.";
            }
            catch (Exception exception) { StatusMessage = ValidationService.GetUserFriendlyError(exception, "Не удалось добавить товар в корзину."); }
        }
    }
}
