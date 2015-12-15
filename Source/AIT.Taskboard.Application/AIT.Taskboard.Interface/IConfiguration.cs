using System;
using System.Collections.Generic;

namespace AIT.Taskboard.Interface
{
    public interface IConfiguration : ICloneable
    {
        string TeamProjectCollection { get; set; }
        string TeamProject { get; set; }
        List<string> ChildItems { get; }
        List<string> BacklogItems { get; }
        List<ICustomState> States { get; }
        string SortFieldName { get; set; }
        SortDirection SortDirection { get; set; }
        string RowSummaryFieldName { get; set; }
        string ColumnSummaryFieldName { get; set; }
        string LinkType { get; set; }
        Guid QueryId { get; set; }
        string ReportId { get; set; }
        bool IsAssociatedWithTeamProject { get; }
        List<Microsoft.Reporting.WinForms.ReportParameter> ReportParameters { get; }

        /// <summary>
        /// IsChecked property of AutoRefresh ToggleBox.
        /// </summary>
        bool IsAutoRefreshChecked { get; set; }
        /// <summary>
        /// Delay between auto refreshes. Determined in seconds.
        /// </summary>
        int AutoRefreshDelay { get; set; }

        bool HideColumnSummaryFieldName { get; set; }
        WorkItemSize WorkItemSize { get; set; }
        bool HideReportViewer { get; set; }
    }
}