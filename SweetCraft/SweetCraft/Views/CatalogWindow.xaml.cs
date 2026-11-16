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
            if (user == null) throw new ArgumentNullException(nameof(user));
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
        private void AdminButton_Click(object sender, RoutedEventArgs e) => new AdminWindow(currentUser) { Owner = this }.ShowDialog();
    }
}
