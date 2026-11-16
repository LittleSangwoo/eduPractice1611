using SweetCraft.Data;
using SweetCraft.ViewModels;
using System.Windows;

namespace SweetCraft.Views
{
    public partial class CartWindow : Window
    {
        public CartWindow(Users user)
        {
            InitializeComponent();
            DataContext = new CartViewModel(user);
        }
    }
}
