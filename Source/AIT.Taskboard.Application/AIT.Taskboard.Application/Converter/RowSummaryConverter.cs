using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using AIT.Taskboard.Interface;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using AIT.Taskboard.Application.Helper;

namespace AIT.Taskboard.Application.Converter
{
    internal class RowSummaryConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var items = new List<RowSummaryItem>();
            if (values.Length == 8)
            {
                var workItems = values[0] as IList<WorkItem>;
                var states = values[1] as IList<ICustomState>;
                var summaryField = values[2] as FieldDefinition;
                var selectedFilter = values[3] as string;
                var areaFilter = values[4] as string;
                var iterationFilter = values[5] as string;
                var width = (double)values[6];
                var idTitleFilter = values[7] as string;
                if (workItems != null && workItems.Count > 0 && states != null)
                {
                    if (summaryField == null)
                    {
                        foreach (var state in states)
                        {
                            var workItemsInCurrentState = workItems.GetWorkItemsByState(state);
                            var percent = workItems.Count > 0 ? (width / workItems.Count) * workItemsInCurrentState.Count() : 0;
                            if (percent > 0)
                            {
                                var item = new RowSummaryItem(new SolidColorBrush(state.Color), percent);
                                if (IsFiltered(selectedFilter, areaFilter, iterationFilter, idTitleFilter))
                                {
                                    item.OpacityWidth = (double)(GetFilteredWorkItems(workItemsInCurrentState, selectedFilter, areaFilter, iterationFilter, idTitleFilter)).Count() / workItemsInCurrentState.Count();
                                }
                                items.Add(item);
                            }
                        }
                    }
                    else
                    {
                        foreach (var state in states)
                        {
                            var workItemsInCurrentState = workItems.GetWorkItemsByState(state);
                            if (summaryField.FieldType == FieldType.Integer)
                            {
                                var percent = (width / GetWorkItemsWithSummary(workItems, summaryField).Select(w => (int)w.Fields[summaryField.Name].Value).Sum()) * 
                                    GetWorkItemsWithSummary(workItemsInCurrentState, summaryField).Select(w => (int)w.Fields[summaryField.Name].Value).Sum();
                                if (percent > 0)
                                {
                                    var item = new RowSummaryItem(new SolidColorBrush(state.Color), percent);
                                    if (IsFiltered(selectedFilter, areaFilter, iterationFilter, idTitleFilter))
                                    {
                                        item.OpacityWidth = GetFilteredWorkItems(workItemsInCurrentState, summaryField, selectedFilter, areaFilter, iterationFilter, idTitleFilter).Select(w => (int)w.Fields[summaryField.Name].Value).Sum() /
                                            GetWorkItemsWithSummary(workItemsInCurrentState, summaryField).Select(w => (int) w.Fields[summaryField.Name].Value).Sum();
                                    }
                                    items.Add(item);
                                }
                            }
                            if (summaryField.FieldType == FieldType.Double)
                            {
                                var percent = (width / GetWorkItemsWithSummary(workItems, summaryField).Select(w => (double)w.Fields[summaryField.Name].Value).Sum()) * GetWorkItemsWithSummary(workItemsInCurrentState, summaryField).Select(w => (double)w.Fields[summaryField.Name].Value).Sum();
                                if (percent > 0)
                                {
                                    var item = new RowSummaryItem(new SolidColorBrush(state.Color), percent);
                                    if (IsFiltered(selectedFilter, areaFilter, iterationFilter, idTitleFilter))
                                    {
                                        item.OpacityWidth = (double)(GetFilteredWorkItems(workItemsInCurrentState, summaryField, selectedFilter, areaFilter, iterationFilter, idTitleFilter).Select(w => (double)w.Fields[summaryField.Name].Value).Sum() / 
                                            GetWorkItemsWithSummary(workItemsInCurrentState, summaryField).Select(w => (double)w.Fields[summaryField.Name].Value).Sum());
                                    }
                                    items.Add(item);
                                }
                            }
                        }
                    }
                }
            }
            return items;
        }

        private IEnumerable<WorkItem> GetWorkItemsWithSummary(IEnumerable<WorkItem> workItems, FieldDefinition summaryField)
        {
            return workItems.Where(w => w.Fields.Contains(summaryField.Name) && w.Fields[summaryField.Name].Value != null);
        }

        private IEnumerable<WorkItem> GetFilteredWorkItems(IEnumerable<WorkItem> workItems, string assignedToFilter, string areaFilter, string iterationFilter, string idTitleFilter)
        {
            if (assignedToFilter != RibbonComboBoxConverter.FILTER_ALL)
                workItems = workItems.Where(w => (string)w.Fields[CoreField.AssignedTo].Value == assignedToFilter);
            if (!String.IsNullOrEmpty(areaFilter))
                workItems = workItems.Where(w => w.AreaPath.StartsWith(areaFilter));
            if (!String.IsNullOrEmpty(iterationFilter))
                workItems = workItems.Where(w => w.IterationPath.StartsWith(iterationFilter));
            if (!String.IsNullOrEmpty(idTitleFilter))
                workItems = workItems.Where(w => (w.Id.ToString().Contains(idTitleFilter) || w.Title.ToLowerInvariant().Contains(idTitleFilter.ToLowerInvariant())));

            return workItems;
        }

        private IEnumerable<WorkItem> GetFilteredWorkItems(IEnumerable<WorkItem> workItems, FieldDefinition summaryField, string assignedToFilter, string areaFilter, string iterationFilter, string idTitleFilter)
        {
            var workItemsWithSummary = GetWorkItemsWithSummary(workItems, summaryField);
            return GetFilteredWorkItems(workItemsWithSummary, assignedToFilter, areaFilter, iterationFilter, idTitleFilter);
        }

        private bool IsFiltered(string assignedToFilter, string areaFilter, string iterationFilter, string idTitleFilter)
        {
            return assignedToFilter != RibbonComboBoxConverter.FILTER_ALL || !String.IsNullOrEmpty(areaFilter) || !String.IsNullOrEmpty(iterationFilter) || !String.IsNullOrEmpty(idTitleFilter);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
