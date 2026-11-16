using SweetCraft.Data;
using SweetCraft.ViewModels;
using SweetCraft.Views;
using System.Windows;

namespace SweetCraft
{
    public partial class MainWindow : Window
    {
        private readonly AuthViewModel viewModel;
        public Users AuthenticatedUser { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            viewModel = new AuthViewModel();
            DataContext = viewModel;
            viewModel.LoginSucceeded += OnLoginSucceeded;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e) => viewModel.Password = passwordBox.Password;
        private void RegistrationPasswordBox_PasswordChanged(object sender, RoutedEventArgs e) => viewModel.RegistrationPassword = registrationPasswordBox.Password;

        private void OnLoginSucceeded(Users user)
        {
            AuthenticatedUser = user;
            OpenCatalog(user);
        }

        private void OpenCatalog(Users user)
        {
            var catalog = new CatalogWindow(user);
            Application.Current.MainWindow = catalog;
            catalog.Show();
            Close();
        }
    }
}
