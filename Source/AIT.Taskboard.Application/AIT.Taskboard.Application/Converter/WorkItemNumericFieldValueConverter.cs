using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace AIT.Taskboard.Application.Converter
{
    public class WorkItemNumericFieldValueConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // The converter receives the work item as value and the field to get as parameter
            var wi = value as WorkItem;
            // If there is no work item there will also be now value
            if (wi == null) return Binding.DoNothing;
            var fieldName = parameter as string;
            // if there is no field name there will also be no value
            if (string.IsNullOrEmpty(fieldName)) return Binding.DoNothing;
            // if there is no field there will also be no value
            if (!wi.Fields.Contains(fieldName)) return Binding.DoNothing;
            var fieldValue = wi.Fields[fieldName].Value;
            if (fieldValue == null) return 0;
            return fieldValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
