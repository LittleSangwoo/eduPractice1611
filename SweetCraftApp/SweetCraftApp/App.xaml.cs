using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SweetCraftApp
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"Упс! Произошла непредвиденная ошибка:\n{e.Exception.Message}\n\nПожалуйста, обратитесь к администратору.",
                            "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true; // Предотвращаем падение программы
        }
    }
}
