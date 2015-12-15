using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Microsoft.Windows.Controls.Ribbon;

namespace AIT.Taskboard.Application.Converter
{
    internal class RibbonComboBoxConverter : IValueConverter
    {
        public const string FILTER_ALL = "<All>";
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var items = new List<RibbonGalleryItem>();
            var values = value as IList<string>;
            if (values != null)
            {
                var empty = new RibbonGalleryItem();
                empty.Content = FILTER_ALL;
                items.Add(empty);
                var orderedValues = values.OrderBy(item => item);
                foreach (var v in orderedValues)
                {
                    if (!string.IsNullOrEmpty(v))
                    {
                        var item = new RibbonGalleryItem();
                        item.Content = v;
                        items.Add(item);
                    }
                }
            }
            return items;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
