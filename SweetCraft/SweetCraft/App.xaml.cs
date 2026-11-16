using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SweetCraft
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            base.OnStartup(e);
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ShowError("Произошла непредвиденная ошибка. Операция не выполнена. Проверьте данные и повторите действие.");
            e.Handled = true;
        }

        private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            Dispatcher.BeginInvoke(new Action(() => ShowError("Не удалось выполнить фоновую операцию. Проверьте данные и подключение к базе данных.")));
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ShowError("Произошла критическая ошибка. Приложение необходимо перезапустить.");
        }

        private static void ShowError(string message)
        {
            MessageBox.Show(message, "SweetCraft", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
