using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Windows;
using AIT.Taskboard.Interface;

namespace AIT.Taskboard.Application.Converter
{
    public class FilterVisibilityConverter : IMultiValueConverter
    {
        private const string AssignedTo = "Assigned To";
        
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length == 5)
                if (values[4] is WorkItem)
                {
                    var assignedToFilter = values[0] as string;
                    string areaFilter = values[1] as string;
                    string iterationFilter = values[2] as string;
                    var workItem = (WorkItem)values[4];
                    var idTitleFilter = values[3] as string;
                    
                    if (assignedToFilter != RibbonComboBoxConverter.FILTER_ALL && 
                        (string)workItem.Fields[AssignedTo].Value != assignedToFilter)
                        return Visibility.Collapsed;

                    if (!String.IsNullOrEmpty(areaFilter) && !workItem.AreaPath.StartsWith(areaFilter))
                        return Visibility.Collapsed;

                    if (!String.IsNullOrEmpty(iterationFilter) && !workItem.IterationPath.StartsWith(iterationFilter))
                        return Visibility.Collapsed;

                    if (!String.IsNullOrEmpty(idTitleFilter) && !(workItem.Id.ToString().Contains(idTitleFilter) || workItem.Title.ToLowerInvariant().Contains(idTitleFilter.ToLowerInvariant())))
                        return Visibility.Collapsed;
                    
                    return Visibility.Visible;
                }
            if(values.Length == 6)
                if (values[5] is IBacklogChildren)
                {
                    string assignedToFilter = values[0] as string;
                    string childrenFilter = values[1] as string;
                    string areaFilter = values[2] as string;
                    string iterationFilter = values[3] as string;
                    var item = (IBacklogChildren)values[5];
                    var idTitleFilter = values[4] as string;
                    switch (childrenFilter)
                    {
                        case "ItemsWithChildren":
                            if (item.Children.Count == 0)
                                return Visibility.Collapsed;
                            break;
                        case "ItemsWithoutChildren":
                            if(item.Children.Count > 0) 
                                return Visibility.Collapsed;
                            break;
                        default: //no filter
                            break;
                    }
                    if (assignedToFilter != RibbonComboBoxConverter.FILTER_ALL &&
                        (string)item.Backlog.Fields[AssignedTo].Value != assignedToFilter &&
                        item.Children.Where(w => (string)w.Fields[AssignedTo].Value == assignedToFilter).Count() == 0)
                            return Visibility.Collapsed;
                    
                    if (!String.IsNullOrEmpty(areaFilter) &&
                        !item.Backlog.AreaPath.StartsWith(areaFilter) &&
                        item.Children.Where(w => w.AreaPath.StartsWith(areaFilter)).Count() == 0)
                            return Visibility.Collapsed;
                    
                    if (!String.IsNullOrEmpty(iterationFilter) &&
                        !item.Backlog.IterationPath.StartsWith(iterationFilter) &&
                        item.Children.Where(w => w.IterationPath.StartsWith(iterationFilter)).Count() == 0)
                            return Visibility.Collapsed;

                    if (!String.IsNullOrEmpty(idTitleFilter) &&
                      !(item.Backlog.Id.ToString().Contains(idTitleFilter) || (item.Backlog.Title.ToLowerInvariant().Contains(idTitleFilter.ToLowerInvariant()))) &&
                      item.Children.Where(w => (w.Id.ToString().Contains(idTitleFilter) || w.Title.ToLowerInvariant().Contains(idTitleFilter.ToLowerInvariant()))).Count() == 0)
                        return Visibility.Collapsed;
                    
                    return Visibility.Visible;
                }
            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
