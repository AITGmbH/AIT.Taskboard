using System;
using System.Windows.Data;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace AIT.Taskboard.Application.Converter
{
    public class TimeInStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // The bound value must be a work item
            var item = value as WorkItem;
            if (item == null) return Binding.DoNothing;

            // The parameter identifies what to return
            var requestedReturnValue = parameter as string;
            var enterTimeFieldName = GetEnterTimeFieldName(item);
            if (!string.IsNullOrEmpty(enterTimeFieldName))
            {
                var enterTimeField = item.Fields[enterTimeFieldName];
                if (enterTimeField.Value == null) return Binding.DoNothing;
                var enterTime = (DateTime) enterTimeField.Value;
                var timespan =DateTime.Now - enterTime;
                switch (requestedReturnValue)
                {
                    case "TotalDays":
                        return Math.Round(timespan.TotalDays, 1);
                    case "TotalHours":
                        return Math.Round(timespan.TotalHours, 1);
                    case "TotalMinutes":
                        return Math.Round(timespan.TotalMinutes, 1);
                    case "ExactTimespan":
                        return timespan;
                    default:
                        return timespan;
                }
            }

            return Binding.DoNothing;
        }

        private static string GetEnterTimeFieldName(WorkItem item)
        {
            // Get the current state from the work item
            var field = item.Fields[CoreField.State];
            var stateName = field.Value as string;
            
            if (!string.IsNullOrEmpty(stateName))
            {
                const string enterPattern = "{0} Enter Time";
                // string leavePattern = "{0} Leave Time";
                var enterFieldName = string.Format(enterPattern, stateName);
                if (item.Fields.Contains(enterFieldName))
                {
                    return enterFieldName;
                }                
            }
            if (item.Fields.Contains("State Change Date"))
                return "State Change Date";
            // Convert the current state into a field name for the enter time
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
