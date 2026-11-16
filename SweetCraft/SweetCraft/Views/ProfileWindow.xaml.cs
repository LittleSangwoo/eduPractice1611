using SweetCraft.Data;
using SweetCraft.ViewModels;
using System.Windows;

namespace SweetCraft.Views
{
    public partial class ProfileWindow : Window
    {
        public ProfileWindow(Users user)
        {
            InitializeComponent();
            DataContext = new ProfileViewModel(user);
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e) => DialogResult = true;
    }
}
