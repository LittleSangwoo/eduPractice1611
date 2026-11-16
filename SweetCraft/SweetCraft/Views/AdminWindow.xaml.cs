using SweetCraft.Data;
using SweetCraft.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SweetCraft.Views
{
    public partial class AdminWindow : Window
    {
        private readonly AdminViewModel viewModel;

        public AdminWindow(Users user)
        {
            InitializeComponent();
            viewModel = new AdminViewModel(user);
            DataContext = viewModel;
            Closed += AdminWindow_Closed;
        }

        private void AdminWindow_Closed(object sender, EventArgs e) => viewModel.Dispose();

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!RecordsGrid.CommitEdit(DataGridEditingUnit.Cell, true) || !RecordsGrid.CommitEdit(DataGridEditingUnit.Row, true))
            {
                MessageBox.Show("Исправьте значения в выделенной строке перед сохранением.", "Проверка данных", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            viewModel.SaveCommand.Execute(null);
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            Type type = Nullable.GetUnderlyingType(e.PropertyType) ?? e.PropertyType;
            if (type != typeof(string) && type != typeof(int) && type != typeof(decimal) && type != typeof(byte) && type != typeof(bool) && type != typeof(DateTime))
            {
                e.Cancel = true;
                return;
            }

            string keyProperty = viewModel.SelectedTable == "Корзина (CartItems)" ? "CartItemId"
                : viewModel.SelectedTable == "Категории (Categories)" ? "CategoryId"
                : viewModel.SelectedTable == "Украшения (Decorations)" ? "DecorationId"
                : viewModel.SelectedTable == "Способы доставки (DeliveryMethods)" ? "DeliveryMethodId"
                : viewModel.SelectedTable == "Начинки (Fillings)" ? "FillingId"
                : viewModel.SelectedTable == "Позиции заказов (OrderItems)" ? "OrderItemId"
                : viewModel.SelectedTable == "Заказы (Orders)" ? "OrderId"
                : viewModel.SelectedTable == "Статусы заказов (OrderStatuses)" ? "StatusId"
                : viewModel.SelectedTable == "История статусов (OrderStatusHistory)" ? "HistoryId"
                : viewModel.SelectedTable == "Способы оплаты (PaymentMethods)" ? "PaymentMethodId"
                : viewModel.SelectedTable == "Товары (Products)" ? "ProductId"
                : viewModel.SelectedTable == "Отзывы (Reviews)" ? "ReviewId"
                : viewModel.SelectedTable == "Роли (Roles)" ? "RoleId"
                : viewModel.SelectedTable == "Пользователи (Users)" ? "UserId"
                : null;
            if (e.PropertyName == keyProperty && e.Column is DataGridBoundColumn column) column.IsReadOnly = true;
        }
    }
}
