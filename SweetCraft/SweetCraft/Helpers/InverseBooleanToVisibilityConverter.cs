using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SweetCraft.Helpers
{
    public sealed class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public bool Invert { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool visible = value is bool && (bool)value;
            if (Invert || (parameter != null && string.Equals(parameter.ToString(), "Inverse", StringComparison.OrdinalIgnoreCase))) visible = !visible;
            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = value is Visibility && (Visibility)value == Visibility.Visible;
            return Invert || (parameter != null && string.Equals(parameter.ToString(), "Inverse", StringComparison.OrdinalIgnoreCase)) ? !result : result;
        }
    }
}
