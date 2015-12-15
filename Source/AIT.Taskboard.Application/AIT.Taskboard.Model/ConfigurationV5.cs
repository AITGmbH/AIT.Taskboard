using System;
using System.Xml.Serialization;
using AIT.Taskboard.Interface;

namespace AIT.Taskboard.Model
{
    [Serializable]
    public class ConfigurationV5 : ConfigurationV4, IConfiguration
    {
        [XmlAnyElement]
        public bool HideReportViewer { get; set; }

        public ConfigurationV5()
        {
            HideReportViewer = true;
        }

        public void CopyFromConfigurationOld4(ConfigurationV4 configOld4)
        {
            CopyFromConfigurationOld3(configOld4);
            HideColumnSummaryFieldName = configOld4.HideColumnSummaryFieldName;
        }

        public object Clone()
        {
            var result = new ConfigurationV5 {TeamProjectCollection = TeamProjectCollection, TeamProject = TeamProject};
            result.ChildItems.AddRange(ChildItems);
            result.BacklogItems.AddRange(BacklogItems);
            foreach (var state in States)
            {
                result.States.Add(state.Clone() as ICustomState);
            }
            result.SortFieldName = SortFieldName;
            result.SortDirection = SortDirection;
            result.RowSummaryFieldName = RowSummaryFieldName;
            result.ColumnSummaryFieldName = ColumnSummaryFieldName;
            result.LinkType = LinkType;
            result.QueryId = QueryId;
            result.ReportId = ReportId;
            result.ReportParameters.AddRange(ReportParameters);
            result.IsAutoRefreshChecked = IsAutoRefreshChecked;
            result.AutoRefreshDelay = AutoRefreshDelay;
            result.HideColumnSummaryFieldName = HideColumnSummaryFieldName;
            result.WorkItemSize = WorkItemSize;
            result.HideReportViewer = HideReportViewer;
            return result;
        }
    }
}
