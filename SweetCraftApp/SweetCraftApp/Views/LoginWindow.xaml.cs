using SweetCraftApp.Data;
using SweetCraftApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SweetCraftApp.Views
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text;
            string password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Введите логин и пароль.");
                return;
            }

            using (var context = new SweetCraftDBEntities())
            {
                var authService = new AuthService(context);

                if (authService.Login(login, password))
                {
                    var currentUser = AuthService.CurrentUser;
                    MessageBox.Show($"Добро пожаловать, {currentUser.FirstName}!\nВаша роль: {currentUser.RoleId}");

                    // 1. Создаем и показываем новое окно в зависимости от роли
                    Window nextWindow;
                    if (currentUser.RoleId == 4 || currentUser.RoleId == 3) // Админ или Менеджер
                    {
                        nextWindow = new AdminPanelWindow();
                    }
                    else // Обычный пользователь
                    {
                        nextWindow = new ProductCatalogWindow();
                    }

                    // 2. Устанавливаем новое окно как главное для приложения, чтобы оно не закрылось
                    Application.Current.MainWindow = nextWindow;

                    // 3. Показываем новое окно
                    nextWindow.Show();

                    // 4. Только теперь закрываем окно авторизации
                    this.Close();
                }
                else
                {
                    ShowError("Неверный логин или пароль.");
                }
            }
        }

        private void BtnGuest_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Вход в режиме гостя.");

            // Открываем каталог для гостя
            var catalogWindow = new ProductCatalogWindow();
            Application.Current.MainWindow = catalogWindow;
            catalogWindow.Show();

            this.Close();
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            txtError.Visibility = Visibility.Visible;
        }
    }
}
