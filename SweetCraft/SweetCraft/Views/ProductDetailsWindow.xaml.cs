using SweetCraft.ViewModels;
using System;
using System.Windows;

namespace SweetCraft.Views
{
    public partial class ProductDetailsWindow : Window
    {
        private readonly Action addToCart;

        public ProductDetailsWindow(ProductCardViewModel product, Action addToCart)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));
            InitializeComponent();
            this.addToCart = addToCart;
            DataContext = product;
            AddToCartButton.IsEnabled = addToCart != null;
            if (addToCart == null) AddToCartButton.Content = "Войдите, чтобы заказать";
        }

        private void AddToCartButton_Click(object sender, RoutedEventArgs e)
        {
            addToCart?.Invoke();
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
    }
}
