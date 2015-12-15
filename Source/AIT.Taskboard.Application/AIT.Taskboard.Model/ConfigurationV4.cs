using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using AIT.Taskboard.Interface;

namespace AIT.Taskboard.Model
{
    [Serializable]
    public class ConfigurationV4: ConfigurationV3
    {
        const WorkItemSize defaultWorkItemSize = WorkItemSize.Medium;

        [XmlAnyElement]
        public WorkItemSize WorkItemSize { get; set; }

        public ConfigurationV4()
            : base()
        {
            WorkItemSize = defaultWorkItemSize;
        }

        public void CopyFromConfigurationOld3(ConfigurationV3 configOld3)
        {
            CopyFromConfigurationOld2(configOld3);
            HideColumnSummaryFieldName = configOld3.HideColumnSummaryFieldName;
        }

        public object Clone()
        {
            ConfigurationV4 result = new ConfigurationV4();
            result.TeamProjectCollection = TeamProjectCollection;
            result.TeamProject = TeamProject;
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
            return result;
        }
    }
}
