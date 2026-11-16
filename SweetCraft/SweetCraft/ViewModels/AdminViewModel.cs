using SweetCraft.Data;
using SweetCraft.Helpers;
using SweetCraft.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SweetCraft.ViewModels
{
    public sealed class AdminViewModel : ObservableObject, IDisposable
    {
        private readonly AdminService adminService;
        private readonly ImageService imageService = new ImageService();
        private string selectedTable;
        private object selectedRow;
        private string message;

        public AdminViewModel(Users user)
        {
            if (user == null || user.Roles == null || (user.Roles.RoleName != "Менеджер" && user.Roles.RoleName != "Администратор")) throw new UnauthorizedAccessException("Недостаточно прав для управления таблицами.");
            IsAdministrator = user.Roles.RoleName == "Администратор";
            adminService = new AdminService(user);
            TableNames = new ObservableCollection<string>(adminService.TableNames);
            Rows = new ObservableCollection<object>();
            TableChangedCommand = new RelayCommand(_ => LoadTable());
            AddCommand = new RelayCommand(_ => AddRow());
            SaveCommand = new RelayCommand(_ => Save());
            DeleteCommand = new RelayCommand(_ => Delete(), _ => IsAdministrator && SelectedRow != null);
            ChooseImageCommand = new RelayCommand(_ => ChooseImage(), _ => SelectedRow is Products);
            SelectedTable = TableNames.Count > 0 ? TableNames[0] : null;
        }

        public bool IsAdministrator { get; }
        public ObservableCollection<string> TableNames { get; }
        public ObservableCollection<object> Rows { get; }
        public ICommand TableChangedCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ChooseImageCommand { get; }
        public string SelectedTable { get => selectedTable; set { if (SetProperty(ref selectedTable, value)) LoadTable(); } }
        public object SelectedRow { get => selectedRow; set { if (SetProperty(ref selectedRow, value)) CommandManager.InvalidateRequerySuggested(); } }
        public string Message { get => message; private set => SetProperty(ref message, value); }

        private void LoadTable()
        {
            if (adminService == null || string.IsNullOrWhiteSpace(SelectedTable)) return;
            try
            {
                Rows.Clear();
                foreach (object row in adminService.Load(SelectedTable)) Rows.Add(row);
                SelectedRow = null;
                Message = "Загружено записей: " + Rows.Count;
            }
            catch (Exception exception) { Message = ValidationService.GetUserFriendlyError(exception, "Не удалось загрузить таблицу."); }
        }

        private void AddRow()
        {
            try
            {
                object row = adminService.Add(SelectedTable);
                Rows.Add(row);
                SelectedRow = row;
                Message = "Новая запись добавлена в таблицу. Заполните обязательные поля и нажмите «Сохранить».";
            }
            catch (Exception exception) { Message = ValidationService.GetUserFriendlyError(exception, "Не удалось создать запись."); }
        }

        private void Save()
        {
            try { adminService.SaveChanges(); Message = "Изменения сохранены."; LoadTable(); }
            catch (Exception exception) { Message = ValidationService.GetUserFriendlyError(exception, "Не удалось сохранить изменения."); }
        }

        private void Delete()
        {
            try { adminService.Delete(SelectedTable, SelectedRow); adminService.SaveChanges(); Message = "Запись удалена."; LoadTable(); }
            catch (Exception exception) { Message = ValidationService.GetUserFriendlyError(exception, "Не удалось удалить запись."); }
        }

        private void ChooseImage()
        {
            try
            {
                string relativePath = imageService.SelectAndCopyProductImage();
                if (relativePath == null) return;
                ((Products)SelectedRow).ImagePath = relativePath;
                Message = "Изображение скопировано в ProductImages. Сохраните изменения.";
                OnPropertyChanged(nameof(Rows));
            }
            catch (Exception exception) { Message = ValidationService.GetUserFriendlyError(exception, "Не удалось скопировать изображение."); }
        }

        public void Dispose() => adminService.Dispose();
    }
}
