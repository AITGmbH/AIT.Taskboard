using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Controls;
using System.Collections;

namespace AIT.Taskboard.Application.Converter
{
    public class EmptyItemConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length == 2)
            {
                var value = values[0];
                var items = values[1] as CompositeCollection;
                if (value == null && items != null && items.Count > 0)
                {
                    return items[0];
                }
                return value;
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is ComboBoxItem)
            {
                return new object[] { null };
            }
            return new object[] { value };
        }

        #endregion
    }
}
