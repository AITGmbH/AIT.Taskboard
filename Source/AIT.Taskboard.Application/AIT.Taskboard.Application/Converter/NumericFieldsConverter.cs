using System;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using AIT.Taskboard.Application.Helper;

namespace AIT.Taskboard.Application.Converter
{
    public class NumericFieldsConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var fields = value as FieldCollection;
            if (fields != null)
            {
                // Only respect the following fields:
                // - numeric editable fields
                // - not fields from black list

                return fields.Cast<Field>().Where(f => f.FieldDefinition.IsEditable && !FieldHelper.FieldBlackList.Contains(f.Name) && (f.FieldDefinition.FieldType == FieldType.Integer || f.FieldDefinition.FieldType == FieldType.Double));
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
