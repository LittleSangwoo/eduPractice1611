using SweetCraft.Data;
using SweetCraft.ViewModels;
using System;
using System.Windows;

namespace SweetCraft.Views
{
    public partial class CatalogWindow : Window
    {
        private readonly Users currentUser;

        public CatalogWindow(Users user)
        {
            InitializeComponent();
            currentUser = user;
            DataContext = new CatalogViewModel(user);
        }

        private void CartButton_Click(object sender, RoutedEventArgs e) => new CartWindow(currentUser) { Owner = this }.ShowDialog();
        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (new ProfileWindow(currentUser) { Owner = this }.ShowDialog() == true)
            {
                var login = new MainWindow();
                Application.Current.MainWindow = login;
                login.Show();
                Close();
            }
        }
        private void ProductDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            var card = (sender as FrameworkElement)?.DataContext as ProductCardViewModel;
            if (card == null) return;

            var viewModel = (CatalogViewModel)DataContext;
            Action addToCart = currentUser == null ? null : (Action)(() => viewModel.AddToCartCommand.Execute(card));
            new ProductDetailsWindow(card, addToCart) { Owner = this }.ShowDialog();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var login = new MainWindow();
            Application.Current.MainWindow = login;
            login.Show();
            Close();
        }

        private void AdminButton_Click(object sender, RoutedEventArgs e) => new AdminWindow(currentUser) { Owner = this }.ShowDialog();
    }
}
