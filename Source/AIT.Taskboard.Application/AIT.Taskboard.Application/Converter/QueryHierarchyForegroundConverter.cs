using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Drawing;
using System.Windows.Media;

namespace AIT.Taskboard.Application.Converter
{
    public sealed class QueryHierarchyForegroundConverter : IMultiValueConverter
    {
        private SolidColorBrush DisabledBrush = new SolidColorBrush(Colors.Gray);
        private SolidColorBrush EnabledBrush = new SolidColorBrush(Colors.Black);

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null) return DependencyProperty.UnsetValue;
            if (values.Length < 2) return DependencyProperty.UnsetValue;

            QueryFolder folder = values[0] as QueryFolder;
            QueryHierarchy hierarchy = values[0] as QueryHierarchy;
            QueryDefinition definition = values[0] as QueryDefinition;

            var brush = EnabledBrush;

            bool isEnabled = values[1] is bool ? (bool)values[1] : false;

            if (!isEnabled)
                return DisabledBrush;

            if (definition != null)
            {
                switch (definition.QueryType)
                {
                    case QueryType.Invalid:
                        brush = DisabledBrush;
                        break;
                    case QueryType.List:
                        brush = DisabledBrush;
                        break;
                    case QueryType.OneHop:
                        brush = EnabledBrush;
                        break;
                    case QueryType.Tree:
                        brush = EnabledBrush;
                        break;
                    default:
                        brush = DisabledBrush;
                        break;
                }
            }

            return brush;
        }

        public object[] ConvertBack(object value, Type[] targetType,
                                  object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
