using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace AIT.Taskboard.Application.Converter
{
    public class BlockedConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var yesNoValue = string.Empty;
            
            // Two options: Either the source is a work item or already a field value
            var workItem = value as WorkItem;
            if (workItem != null)
            {
                // it is a work item. Try to find the field passed in as parameter
                var fieldName = parameter as string;
                if (string.IsNullOrEmpty(fieldName)) return Visibility.Hidden;
                if (workItem.Fields.Contains(fieldName))
                    yesNoValue = workItem.Fields[fieldName].Value as string;
            }
            else
            {
                yesNoValue = value as string;    
            }
            
            return yesNoValue == "Yes" ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}