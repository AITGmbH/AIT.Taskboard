using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace AIT.Taskboard.Application.Converter
{
    public class TitleConverter : IMultiValueConverter
    {
        #region Implementation of IMultiValueConverter

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // No data bound. Return fallback
            if (values == null) return parameter;
            // Not enough data provided. return fallback
            if (values.Length < 4) return parameter;
            // Extract the data
            var defaultTitle = values[0] as string;
            var titleTemplate = values[1] as string;
            var isTrialVersion = values[2] as bool?;

            var isTrial = string.Empty;
            if (null != isTrialVersion && isTrialVersion == true)
                isTrial = "(Trial)";

            // If ther is no title template we just return the default
            if (string.IsNullOrEmpty(titleTemplate)) return string.Format(defaultTitle, isTrial) ?? parameter;
          
            // Check if the third value is a valid string
            var value = values[3] as string;
            if (string.IsNullOrEmpty(value)) return string.Format(defaultTitle, isTrial) ?? parameter;

            // gets whether file is read only
            var isReadOnly = (bool)values[4];
            if (isReadOnly)
            {
                value = string.Format("{0} [{1}]", value, Properties.Resources.ReadOnly);
            }

            return string.Format(titleTemplate, isTrial, value);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
