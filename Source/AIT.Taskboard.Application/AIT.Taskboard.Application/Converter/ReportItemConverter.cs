using System;
using System.Text;
using System.Windows.Data;
using AIT.Taskboard.Interface;

namespace AIT.Taskboard.Application.Converter
{
    internal class ReportItemConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var item = value as IHierarchicalItem;
            if (item != null)
            {
                return GetPath(item);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        private static string GetPath(IHierarchicalItem item)
        {
            var sb = new StringBuilder();
            sb.Append(item.Item.Name);
            if (item.Parent != null)
            {
                sb.Insert(0, GetPath(item.Parent) + "/");
            }
            return sb.ToString();
        }
    }
}
