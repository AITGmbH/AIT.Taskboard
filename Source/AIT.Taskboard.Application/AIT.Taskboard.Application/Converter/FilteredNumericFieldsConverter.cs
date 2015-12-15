using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using AIT.Taskboard.Application.Helper;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace AIT.Taskboard.Application.Converter
{
    public class FilteredNumericFieldsConverter : IMultiValueConverter
    {
        #region Implementation of IMultiValueConverter

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var fields = values[0] as FieldCollection;
            var numericQueryFieldDefinitions = values[1] as List<FieldDefinition>;
            if (fields != null && numericQueryFieldDefinitions != null)
            {
                // Only respect the following fields:
                // - numeric ediable fields
                // - not fields from black list
                // - only fields returned in query
                var queryFields = from field in numericQueryFieldDefinitions select field.Name;
                return fields.Cast<Field>().Where(f => f.FieldDefinition.IsEditable && !FieldHelper.FieldBlackList.Contains(f.Name) && queryFields.Contains(f.Name) && (f.FieldDefinition.FieldType == FieldType.Integer || f.FieldDefinition.FieldType == FieldType.Double));
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}