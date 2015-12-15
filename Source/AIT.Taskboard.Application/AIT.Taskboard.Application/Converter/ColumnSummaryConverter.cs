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
    public class ColumnSummaryConverter : IMultiValueConverter
    {
        
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length == 9)
            {
                var workItems = values[0] as IList<WorkItem>;
                var state = values[1] as ICustomState;
                var summaryField = values[2] as FieldDefinition;
                var hideSummaryField = false;
                try
                {
                    hideSummaryField = (bool)values[3]; //cannot apply "as" on boolean type
                }
                catch { }
                var selectedFilter = values[4] as string;
                var childrenFilter = values[5] as string;
                var areaFilter = values[6] as string;
                var iterationFilter = values[7] as string;
                var idTitleFilter = values[8] as string;
                if (workItems != null && workItems.Count > 0 && state != null)
                {
                    //summary is irrelevant if there are no children
                    //TODO: maybe change the message?
                    if (childrenFilter == "ItemsWithoutChildren")
                        return "(no children)";

                    bool isFiltered = IsFiltered(selectedFilter, areaFilter, iterationFilter, idTitleFilter);
                    string summaryString = isFiltered ? "0/0" : "0";//default value
                    var workItemsInCurrentState = workItems.GetWorkItemsByState(state);

                    if (summaryField == null)
                    {
                        if (!isFiltered)
                            summaryString = workItemsInCurrentState.Count().ToString();
                        else
                        {
                            var filteredItems = GetFilteredWorkItems(workItemsInCurrentState, selectedFilter, areaFilter, iterationFilter, idTitleFilter);
                            summaryString = String.Format("{0}/{1}", filteredItems.Count(), workItemsInCurrentState.Count());
                        }
                        return hideSummaryField ? String.Format("({0})", summaryString) : String.Format("(Count: {0})", summaryString);
                    }
                    else
                    {
                        //summary field is defined
                        var workItemsWithSummaryField = workItemsInCurrentState.Where(w => w.Fields.Contains(summaryField.Name) && w.Fields[summaryField.Name].Value != null);
                        if (workItemsWithSummaryField.Count() > 0)
                        {
                            if (!isFiltered)
                            {
                                if (summaryField.FieldType == FieldType.Integer)
                                {
                                    summaryString = workItemsWithSummaryField.Select(w => (int)w.Fields[summaryField.Name].Value).Sum().ToString();
                                }
                                if (summaryField.FieldType == FieldType.Double)
                                {
                                    summaryString = workItemsWithSummaryField.Select(w => (double)w.Fields[summaryField.Name].Value).Sum().ToString();
                                }
                            }
                            else
                            {
                                var filterworkItemsFiltered = GetFilteredWorkItems(workItemsWithSummaryField, selectedFilter, areaFilter, iterationFilter, idTitleFilter);
                                if (summaryField.FieldType == FieldType.Integer)
                                {
                                    summaryString = String.Format("{0}/{1}", filterworkItemsFiltered.Select(w => (int)w.Fields[summaryField.Name].Value).Sum(),
                                        workItemsWithSummaryField.Select(w => (int)w.Fields[summaryField.Name].Value).Sum());
                                }
                                if (summaryField.FieldType == FieldType.Double)
                                {
                                    summaryString = String.Format("{0}/{1}", filterworkItemsFiltered.Select(w => (double)w.Fields[summaryField.Name].Value).Sum(),
                                        workItemsWithSummaryField.Select(w => (double)w.Fields[summaryField.Name].Value).Sum());
                                }
                            }
                        }
                        return hideSummaryField ? String.Format("({0})", summaryString) : String.Format("({0}: {1})", summaryField.Name, summaryString);
                    }
                }
            }
            return null;
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
