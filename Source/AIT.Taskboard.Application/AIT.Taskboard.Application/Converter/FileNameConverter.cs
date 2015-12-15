using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;

namespace AIT.Taskboard.Application.Converter
{
    public class FileNameConverter :IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var fullPath = value as string;
            
            if (string.IsNullOrEmpty(fullPath))
                return DependencyProperty.UnsetValue;
            
            var info = new FileInfo(fullPath);
            return info.Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}