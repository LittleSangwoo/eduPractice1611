using SweetCraftApp.Data;
using SweetCraftApp.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SweetCraftApp.ViewModels
{
    public class AdminPanelViewModel : ObservableObject
    {
        private readonly SweetCraftDBEntities _context;

        // Коллекция привязана напрямую к Local-кэшу EF6
        public ObservableCollection<Products> ManageableProducts { get; set; }
        public Products SelectedProduct { get; set; }

        public RelayCommand DeleteProductCommand { get; }
        public RelayCommand SaveChangesCommand { get; }
        public RelayCommand UploadImageCommand { get; }

        public AdminPanelViewModel()
        {
            _context = new SweetCraftDBEntities();

            // Загружаем данные в память (EF6)
            _context.Products.Load();
            ManageableProducts = _context.Products.Local;

            // Удаление доступно только Администратору (роль 4)[cite: 3]
            DeleteProductCommand = new RelayCommand(DeleteProduct, p => SelectedProduct != null && Services.AuthService.CurrentUser?.RoleId == 4);
            SaveChangesCommand = new RelayCommand(SaveChanges);
            UploadImageCommand = new RelayCommand(UploadImage, p => SelectedProduct != null);
        }

        private void UploadImage(object obj)
        {
            var fileService = new Services.FileService();
            string newPath = fileService.SelectAndCopyImage();
            if (newPath != null)
            {
                SelectedProduct.ImagePath = newPath;
                OnPropertyChanged(nameof(SelectedProduct));
            }
        }

        private void DeleteProduct(object obj)
        {
            try
            {
                _context.Products.Remove(SelectedProduct);
                _context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                // Перехват запрета каскадного удаления товара, который есть в заказах (ON DELETE NO ACTION)[cite: 3]
                if (ex.InnerException?.InnerException != null && ex.InnerException.InnerException.Message.Contains("FK_OrderItems_Products"))
                {
                    MessageBox.Show("Невозможно удалить товар, так как он присутствует в одном или нескольких заказах.",
                                    "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Warning);

                    // Откатываем состояние объекта Products[cite: 2] в контексте, чтобы он вернулся в таблицу
                    _context.Entry(SelectedProduct).State = EntityState.Unchanged;
                }
                else throw;
            }
        }

        private void SaveChanges(object obj) => _context.SaveChanges();
    }
}
