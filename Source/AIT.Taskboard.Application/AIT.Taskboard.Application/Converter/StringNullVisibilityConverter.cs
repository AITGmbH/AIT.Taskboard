using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AIT.Taskboard.Application.Converter
{
    public class StringNullVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string textValue = value as string;
            if (string.IsNullOrEmpty(textValue))
                return Visibility.Hidden;
            return Visibility.Visible;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}