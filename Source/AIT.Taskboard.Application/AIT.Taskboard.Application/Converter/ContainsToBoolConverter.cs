using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Collections;
using AIT.Taskboard.Interface;

namespace AIT.Taskboard.Application.Converter
{
    public class ContainsToBoolConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length == 2)
            {
                var item = values[0] as string;
                var items = values[1] as IList<string>;
                if (items != null && item != null)
                {
                    return items.Contains(item);
                }
                var states = values[1] as IList<ICustomState>;
                if (states != null)
                {
                    return states.Where(s => s.WorkItemStates.Contains(item)).Count() > 0;
                }
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
