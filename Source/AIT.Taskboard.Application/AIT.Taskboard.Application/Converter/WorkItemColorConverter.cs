using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.Windows.Controls;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using AIT.Taskboard.Interface;

namespace AIT.Taskboard.Application.Converter
{
    internal class WorkItemColorConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length == 2)
            {
                WorkItem currentWorkItem = values[0] as WorkItem;
                var states = values[1] as IList<ICustomState>;
                if (currentWorkItem == null || states == null)
                    return Colors.Gray;

                var matchingCustomState = states.FirstOrDefault(s => s.WorkItemStates.Contains(currentWorkItem.State));
                return matchingCustomState != null ? matchingCustomState.Color : Colors.Gray;
            }
            return Colors.Gray;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
